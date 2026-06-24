import { Component, OnInit, AfterViewInit, signal, inject, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild } from '@angular/core';
import { User, UserRole } from '../../../core/models/user.model';
import { UserService } from '../../../core/services/user';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../../shared/material/material.module';
import { MatTableDataSource } from '@angular/material/table';
import { AuthService } from '../../../core/services/auth';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';



@Component({
  selector: 'app-user-management',
  imports: [
    CommonModule,
    MaterialModule,
    MatSort,
    MatPaginator,
    
  ],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  
})
export class UserManagement implements OnInit, AfterViewInit {
  
  displayedColumns = [
    'fullName',
    'email',
    'role',
    'status',
    'createdAt',
    'actions'
  ];

  dataSource = new MatTableDataSource<User>([]);
  loading = signal(false);
  error = signal('');
  trackById = (_index: number, user: User) => user.id;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  readonly roles: UserRole[] = ['Admin', 'Manager', 'Employee'];
  readonly statuses = ['Active', 'Inactive'];

  private searchText = '';
  private roleFilter = '';
  private statusFilter = '';
  private userService = inject(UserService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit(): void {
    this.loadUsers();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.filterPredicate = (data: User) => {
      const matchesSearch = !this.searchText ||
        data.fullName.toLowerCase().includes(this.searchText) ||
        data.email.toLowerCase().includes(this.searchText);
      const matchesRole = !this.roleFilter ||
        data.role.toLowerCase() === this.roleFilter;
      const matchesStatus = !this.statusFilter ||
        (this.statusFilter === 'active' ? data.isActive : !data.isActive);
      return matchesSearch && matchesRole && matchesStatus;
    };
  }


  loadUsers(): void{
    this.loading.set(true);
    this.userService.getAllUsers().subscribe({
      next: users =>{
        this.dataSource.data = users;
        this.loading.set(false);
        this.cdr.markForCheck();
      },
      error: ()=> {
        this.error.set('Failed to load users');
        this.loading.set(false);
        this.cdr.markForCheck();
      }
    })
  }

  toggleUserStatus(user: User): void{
    const currentUser = this.authService.user();
    if(currentUser?.email === user.email){
      return;
    }
    this.userService.updateUserStatus(user.id, { isActive: !user.isActive })
      .subscribe({
        next: ()=>{
          this.dataSource.data = this.dataSource.data.map(u =>
            u.id === user.id ? { ...u, isActive: !u.isActive } : u
          );
          this.cdr.markForCheck();
        },
        error: ()=>{
          this.error.set('Failed to update user status');
          this.cdr.markForCheck();
        }
      })
  }

  updateUserRole(user: User, newRole: UserRole): void{
    const currentUser = this.authService.user();
    if(currentUser?.email === user.email){
      return;
    }

    if (user.role === newRole) return; 

    this.userService.updateUserRole(user.id, { role: newRole }).subscribe({
      next: ()=>{
        this.dataSource.data = this.dataSource.data.map(u =>
          u.id === user.id ? { ...u, role: newRole } : u
        );
        this.cdr.markForCheck();
      },
      error: ()=>{
        this.error.set('Failed to update user role');
        this.cdr.markForCheck();
      }
    })
  }

  deleteUser(user: User): void{
    const currentUser = this.authService.user();
    if(currentUser?.email === user.email){
      return;
    }

    const admin = this.authService.isAdmin();
    if(admin && user.role === 'Admin'){
      return;
    }
    if(confirm(`Do you want to delete user ${user.fullName}?`)){
      this.userService.deleteUser(user.id).subscribe({
        next: ()=>{
          this.dataSource.data = this.dataSource.data.filter( u => u.id !== user.id);
          this.cdr.markForCheck();
        },
        error: ()=> {
          this.error.set('Failed to delete user');
          this.cdr.markForCheck();
        }
      })
    }
  }

  applyFilter(event: Event){
    this.searchText = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.triggerFilter();
  }

  filterByRole(role: string){
    this.roleFilter = role.toLowerCase();
    this.triggerFilter();
  }

  filterByStatus(status: string){
    this.statusFilter = status.toLowerCase();
    this.triggerFilter();
  }

  private triggerFilter(): void {
    // dataSource only runs filterPredicate when filter string is non-empty
    this.dataSource.filter = (this.searchText || this.roleFilter || this.statusFilter)
      ? '__active__'
      : '';
  }


}
