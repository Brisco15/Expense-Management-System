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
import { EditCategory } from '../edit-category/edit-category';
import { UpdateCategoryBudget } from '../update-category-budget/update-category-budget';
import { BudgetService } from '../../../core/services/budget';
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
    'monthlyBudget',
    'yearlyBudget',
    'totalExpenses',
    'totalAmount',
    'status',
    'createdAt',
    'actions'
  ];
  
  categories: CategoryExpense[]= [];
  dataSource = new MatTableDataSource<CategoryExpense>([]);
  loading = signal(false);
  error = signal('');
  success = signal('');
  trackById = ( _index: number, category: CategoryExpense) => category.categoryId;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  readonly statuses = ['Active', 'Inactive'];
  private searchText = '';

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
    private budgetService: BudgetService,
  ){}

  private showSuccess(message: string): void {
    this.success.set(message);
    this.cdr.markForCheck();
    setTimeout(() => {
      this.success.set('');
      this.cdr.markForCheck();
    }, 2000);
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.filterPredicate = (data: CategoryExpense, _filter: string): boolean => {
        const matchesSearch = !this.searchText ||
          data.categoryName.toLowerCase().includes(this.searchText) ||
          (data.description?.toLowerCase().includes(this.searchText) ?? false) ||
          data.createdAt.toLowerCase().includes(this.searchText)  ||
          (data.monthlyBudget?.toString().includes(this.searchText) ?? false);

        return matchesSearch;
    }
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
    this.categoryService.updateCategoryStatus(category.categoryId, {isActive: !category.isActive}).subscribe({
      next : ()=>{
        this.dataSource.data = [...this.dataSource.data.map(c => 
          c.categoryId === category.categoryId ? { ...c, isActive: !c.isActive } : c
        )];
        this.cdr.markForCheck();
      },
      error : ()=> {
        this.error.set(`Failed to Activate or Deactivate ${category.categoryName}`);
        this.cdr.markForCheck();
      }
    })

  }

  deleteCategory(category: CategoryExpense): void{
    if(!this.isAdmin){
      this.error.set('Only admins can delete category');
      this.cdr.markForCheck();
      return;
    }

    if(confirm(`Do you want to delete category ${category.categoryName}?`)){
      this.categoryService.deleteCategory(category.categoryId).subscribe({
        next: ()=> {
          this.dataSource.data = this.dataSource.data.filter(c =>c.categoryId !== category.categoryId);
          this.cdr.markForCheck();
        },
        error: ()=> {
          this.error.set('Failed to delete user.');
          this.cdr.markForCheck();
        }
      })
    }
  }

  updateCategory(category: CategoryExpense): void{
    if(!this.isAdmin){
      this.error.set('Only admins can update a category. ');
      this.cdr.markForCheck();
      return;
    }

    const dialogRef = this.dialog.open(EditCategory, {
      width: '400px',
      height: '300px',
      data: { category }
    });

    dialogRef.afterClosed().subscribe(result =>{
      if(!result) return;

      const categoryToEdit = this.categories.find(c => c.categoryId === category.categoryId);
      if(!categoryToEdit){
        this.error.set('Category not found.');
        return;
      }

      const updatedCategory = {
        categoryId: category.categoryId,
        categoryName: result.categoryName,
        description: result.description,
        updatedAt: new Date().toISOString(),
        isActive: true,
      };

      this.categoryService.updateCategory(category.categoryId, updatedCategory).subscribe({
        next: ()=> {
          this.showSuccess('Category was successfully updated.');
          this.loadCategories();
        },
        error: (err: any)=>{
          if (err.status === 409) {
            this.error.set(`A category named '${result.categoryName}' already exists.`);
          } else if (err.status === 400) {
            this.error.set('Invalid data. Please check your inputs.');
          } else {
            this.error.set('Failed to update category. Please try again.');
          }
          this.cdr.markForCheck();

        }
      })
    })


  }

  manageBudget(category: CategoryExpense){
    if(!this.isAdmin){
      this.error.set('Only admins can update budgets');
      this.cdr.markForCheck();
      return;
    }

    const dialogRef = this.dialog.open(UpdateCategoryBudget, {
      width: '400px',
      height: '300px',
      data: { category }
    });

    dialogRef.afterClosed().subscribe(result =>{
      if(!result) return;

      const categoryToManage = this.categories.find(c => c.categoryId === category.categoryId);
      if(!categoryToManage){
        this.error.set('Category not found.');
        return;
      };

      const updatedCategoryBudget = {
        categoryId: category.categoryId,
        monthlyBudget: result.monthlyBudget,
        yearlyBudget: result.yearlyBudget,
      };

      this.budgetService.updateCategoryBudget(category.categoryId, updatedCategoryBudget).subscribe({
        next: ()=>{
          this.showSuccess('Budget was successfully updated.');
          this.loadCategories();

        },
        error: ()=>{
          this.error.set('failed to update the budget');
          this.cdr.markForCheck();
        }
      })
    })

  }

  applyFilter(event: Event){
    this.searchText = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.triggerFilter();

  }

  private triggerFilter(): void{
    this.dataSource.filter = this.searchText ? '__active__' : '';
  }

  addCategory(){

    if (!this.isAdmin) {
      this.error.set('Only admins can create categories.');
      this.cdr.markForCheck();
      return;
    }
    const dialogRef = this.dialog.open(CreateCategory, {
      width: '400px',
      height: '300px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) return;

      const categoryExists = this.categories.some(c => c.categoryName === result.categoryName);
      if (categoryExists) {
        this.error.set(`A category with '${result.categoryName}' already exists.`);
        return;
      }

      const newCategory = {
        categoryName: result.categoryName,
        description: result.description,
        totalExpenses: 0,
        totalAmount: 0,
        isActive: true,
        createdAt: new Date().toISOString(),
      };

      this.categoryService.createCategory(newCategory).subscribe({
        next: () => {
          this.showSuccess('Category was created successfully.');
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
