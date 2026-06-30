import { CommonModule } from '@angular/common';
import { Component, AfterViewInit, OnInit, ViewChild, signal, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortHeader } from '@angular/material/sort';
import { ExpenseService } from '../../../core/services/expense';
import { Expense } from '../../../core/models/expenses.model';
import { CategoryExpense } from '../../../core/models/category-expense.model';
import { CategoryService } from '../../../core/services/category';
import { MaterialModule } from '../../../shared/material/material.module';
import { MatDialog } from '@angular/material/dialog';
import { CreateExpenseDialog } from '../create-expense-dialog/create-expense-dialog';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { User } from '../../../core/models/user.model';
import { UpdateExpenseStatus } from '../update-expense-status/update-expense-status';
import { EditExpense } from '../edit-expense/edit-expense';


@Component({
  selector: 'app-expense-list',
  imports: [
    CommonModule,
    MaterialModule,
    MatSort,
    MatSortHeader,
    MatPaginator,
    
],
  templateUrl: './expense-list.html',
  styleUrl: './expense-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExpenseList implements OnInit, AfterViewInit { 

  displayedColumns = [
    'title',
    'category',
    'amount',
    'status',
    'user',
    'expenseDate',
    'actions'
  ];

  expenses: Expense[] = [];
  dataSource = new MatTableDataSource<Expense>([]);
  loading = signal(false);
  error = signal('');
  success = signal('');
  trackById = ( _index: number, expense: Expense) => expense.id;
  http = inject(HttpClient);
  router = inject(Router);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  private expenseService = inject(ExpenseService);
  private categoryService = inject(CategoryService);
  private authService = inject(AuthService);
  categories = signal<CategoryExpense[]>([]);
  users = signal<User[]>([]);
  private cdr = inject(ChangeDetectorRef);
  private searchText = '';
  private categoryFilter = '';

  get isAdmin(): boolean{
    return this.authService.isAdmin()
  }

  get isEmployee():boolean{
    return this.authService.isEmployee()
  }

  constructor(
    private dialog: MatDialog
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
    this.loadExpenses();
    this.loadCategories();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.filterPredicate = (data: Expense, _filter: string): boolean => {
      const matchesSearch = !this.searchText || 
        data.title.toLowerCase().includes(this.searchText) ||
        (data.description?.toLowerCase().includes(this.searchText) ?? false) ||
        data.expenseDate.toString().includes(this.searchText);

      const matchesCategory = !this.categoryFilter ||
        data.category.toLowerCase() === this.categoryFilter;

      return matchesSearch && matchesCategory;
    }
  }
  
  loadExpenses(): void{
    this.loading.set(true);
    this.expenseService.getAllExpenses().subscribe({
      next: expenses => {
        this.dataSource.data = expenses;
        this.loading.set(false);
        this.cdr.markForCheck();
        
      },
      error: ()=> {
        this.error.set('Failed to load Expense list');
        this.loading.set(false);
        this.cdr.markForCheck();
      }
    })

  }

  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: categories => this.categories.set(categories),
      error: () => {
        this.error.set('could not load categories')
      }
    });
  }

  applyFilter(event: Event){
    this.searchText = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.triggerFilter();
  }

  filterByCategory(category: string) {
    this.categoryFilter = category.toLowerCase();
    this.triggerFilter();
  }

  private triggerFilter(): void{
    this.dataSource.filter = (this.searchText || this.categoryFilter)
      ? '__active__' 
      : '';
  }
  
  addExpense(){
    const dialogRef = this.dialog.open(CreateExpenseDialog, {
      width: '400px',
      height: '500px',
      data: { categories: this.categories() }
    });

    dialogRef.afterClosed().subscribe(result =>{
      if(!result) return;

      const expenseExists = this.dataSource.data.some(e => e.title === result.title);
       if(expenseExists){
        this.error.set(`An expense with '${result.title}' already exist.`);
        return;

       }

       const newExpense = {
        title : result.title,
        description: result.description,
        amount: result.amount,
        categoryId: result.categoryId,
        expenseDate: new Date().toISOString(),
        user: this.authService.getUser()
       };

       this.expenseService.create(newExpense).subscribe({
        next: ()=> {
          this.showSuccess('Expense was created successfully.');
          this.loadExpenses();

        },
        error: (err: any)=>{
          if(err.status === 409){
            this.error.set(`An Expenses with title '${result.title}' already exists.`)
          }else if (err.status === 400) {
            this.error.set(' Invalid data.Please check your inputs.')
          }else{
            this.error.set(' failed to create a new Expense. Please try again.')
          }
          this.cdr.markForCheck();
        }
       })
    })
  }

  manageExpense(expense: Expense){
    if(this.isEmployee){
      this.error.set('Only admins & managers can manage the expenses.');
      this.cdr.markForCheck();
      return;
    }

    const dialogRef = this.dialog.open(UpdateExpenseStatus, {
      width: '400px',
      data: { expense }
    });

    dialogRef.afterClosed().subscribe(result =>{
      if(!result) return;

      const approvedExpense = {
        expenseId: expense.id,
        isApproved: result.isApproved,
        rejectionReason: result.rejectionReason
      };
      this.expenseService.approve(expense.id, approvedExpense ).subscribe({
        next: ()=> {
          this.showSuccess('status was successfully updated.');
          this.loadExpenses();
        },
        error: ()=>{
          this.error.set('Failed to update the satus of the expense.');
          this.cdr.markForCheck();
        }
      })
    })

  }

  //TODO
  editExpense(expense: Expense){
    if(this.isEmployee){
      this.error.set('Only admins can update expense. ');
      this.cdr.markForCheck();
      return;
    }

    const dialogRef = this.dialog.open(EditExpense, {
      width: '400px',
      height: '550px',
      data: { expense, categories: this.categories() }
    });

    dialogRef.afterClosed().subscribe(result =>{
      if(!result) return;

      const updatedExpense = {
        id: expense.id,
        title: result.title,
        description: result.description,
        amount: result.amount,
        categoryId: result.categoryId,
        updatedAt: new Date().toISOString(),
        expenseDate: expense.expenseDate,
        createdBy: expense.createdBy,
        status: expense.status
      }

      this.expenseService.update(expense.id, updatedExpense).subscribe({
        next: ()=> {
          if (result.receiptFile) {
            this.expenseService.uploadReceipt(expense.id, result.receiptFile).subscribe({
              next: () => {
                this.showSuccess(`'${result.title}' updated with receipt.`);
                this.loadExpenses();
              },
              error: () => {
                this.showSuccess(`'${result.title}' updated, but receipt upload failed.`);
                this.loadExpenses();
              }
            });
          } else {
            this.showSuccess(`Expense '${result.title}' successfully updated.`);
            this.loadExpenses();
          }
        },
        error: (err: any)=>{
          if(err.status === 409){
            this.error.set( `An expense with this '${result.title}' already exists.` )
          } else if(err.status === 400) {
            const msg = typeof err.error === 'string' ? err.error : 'Invalid data. Please check your inputs.';
            this.error.set(msg);
          } else {
            this.error.set('Failed to edit expense. Please try again.')
          }
          this.cdr.markForCheck();
        }
      })

      


    })


    




  }

  
  deleteExpense(expense: Expense){
    if(!this.isAdmin){
      this.error.set('Only admins can delete expense.');
      this.cdr.markForCheck();
      return;
    }

    if(confirm(`Do you want to delete "${expense.title}"?`)){
      this.expenseService.delete(expense.id).subscribe({
        next: ()=> {
          this.showSuccess('Expense was successfully deleted.')
          this.dataSource.data = this.dataSource.data.filter(e =>e.id !== expense.id);
          this.cdr.markForCheck();
        },
        error: ()=> {
          this.error.set('Failed to delete expense.');
          this.cdr.markForCheck();
        }
      })

    }


  }


}
