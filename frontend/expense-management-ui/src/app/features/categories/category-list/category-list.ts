import { Component, signal, AfterViewInit, OnInit, OnDestroy, ChangeDetectorRef, inject, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { CategoryExpense } from '../../../core/models/category-expense.model';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../../shared/material/material.module';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { CategoryService } from '../../../core/services/category';
import { MatDialog } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { CreateCategory } from '../create-category/create-category';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-category-list',
  imports: [
    CommonModule,
    MaterialModule,
    MatSort,
    MatPaginator,
    
  ],
  templateUrl: './category-list.html',
  styleUrl: './category-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryList implements OnInit, AfterViewInit,OnDestroy {

  displayedColumns = [
    'categoryName',
    'description',
    'createdAt',
    'monthlyBudget',
    'yearlyBudget',
    'totalExpenses',
    'totalAmount',
    'actions'
  ];
  
  categories: CategoryExpense[]= [];
  dataSource = new MatTableDataSource<CategoryExpense>([]);
  loading = signal(false);
  error = signal('');
  trackById = ( _index: number, category: CategoryExpense) => category.categoryId;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  http = inject(HttpClient);
  router = inject(Router);

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  constructor( 
    private dialog: MatDialog,
    private categoryService: CategoryService,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
  ){}

  ngOnInit(): void {
    this.loadCategories();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  private reconnectTableControls(): void {
    setTimeout(() => {
      this.dataSource.sort = this.sort;
      this.dataSource.paginator = this.paginator;
    });
  }

  ngOnDestroy(): void {
    
  }

  loadCategories(): void{
    this.loading.set(true);
    this.categoryService.getAllCategories().subscribe({
      next: categories => {
        this.categories = categories;
        this.dataSource.data = categories;
        this.loading.set(false);
        this.cdr.markForCheck();
        this.reconnectTableControls();
      },
      error: ()=> {
        this.error.set('Failed to load categories');
        this.loading.set(false);
        this.cdr.markForCheck();
      }
    })

  }

  toggleCategoryStatus(category: CategoryExpense): void{

  }

  deleteCategory(category: CategoryExpense): void{

  }

  updateCategory(category: CategoryExpense): void{

  }

  applyFilter(event: Event){

  }

  addCategory(){

    if (!this.isAdmin) {
      this.error.set('Only admins can create categories.');
      this.cdr.markForCheck();
      return;
    }
    const dialogRef = this.dialog.open(CreateCategory, {
      width: '400px',
      height: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) return;

      const categoryExists = this.categories.some(c => c.categoryName === result.categoryName);
      if (categoryExists) {
        alert(`A category with '${result.categoryName}' already exists.`);
        return;
      }

      const newCategory = {
        categoryName: result.categoryName,
        description: result.description,
        monthlyBudget: result.monthlyBudget,
        yearlyBudget: result.monthlyBudget * 12,
        totalExpenses: 0,
        totalAmount: 0,
        isActive: true,
        createdAt: new Date().toISOString(),
      };

      this.categoryService.createCategory(newCategory).subscribe({
        next: () => {
          alert('The new category was created successfully.');
          this.loadCategories();
        },
        error: (err: any) => {
          if (err.status === 409) {
            this.error.set(`A category named '${result.categoryName}' already exists.`);
          } else if (err.status === 400) {
            this.error.set('Invalid data. Please check your inputs.');
          } else {
            this.error.set('Failed to create a new category. Please try again.');
          }
          this.cdr.markForCheck();
        }
      });
    })
  }

}
