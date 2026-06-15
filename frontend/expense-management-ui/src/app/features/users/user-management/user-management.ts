import { Component, OnInit, signal, inject} from '@angular/core';
import { User } from '../../../core/models/user.model';
import { UserService } from '../../../core/services/user';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../../shared/material/material.module';
import { MatTableDataSource } from '@angular/material/table';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-user-management',
  imports: [
    CommonModule,
    MaterialModule,
   //MatSort,
   // MatPaginator,
    //MatSortHeader
  ],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
})
export class UserManagement implements OnInit {
  
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

  private userService = inject(UserService);
  private authService = inject(AuthService);

  ngOnInit(): void {
    
    this.loadUsers();
  }

  loadUsers(): void{
    this.loading.set(true);
    this.userService.getAllUsers().subscribe({
      next: users =>{
        this.dataSource.data = users;
        this.loading.set(false);
      },
      error: ()=> {
        this.error.set('Failed to load users');
        this.loading.set(false);
      }
    })
  }

  toggleUserStatus(user: User): void{
    this.userService.updateUserStatus(user.id, { isActive: !user.isActive })
      .subscribe({
        next: ()=>{
          user.isActive = !user.isActive;
        },
        error: ()=>{
          this.error.set('Failed to update user status');
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

    if(confirm(`Delete user ${user.fullName}?`)){
      this.userService.deleteUser(user.id).subscribe({
        next: ()=>{
          this.dataSource.data = this.dataSource.data.filter( u => u.id !== user.id);
        },
        error: ()=> this.error.set('Failed to delete user') 
      })
    }
  }






}
