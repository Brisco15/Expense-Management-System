import { CommonModule } from '@angular/common';
import { Component, AfterViewInit, OnInit, ViewChild, signal, inject, ChangeDetectorRef } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, MatSortHeader } from '@angular/material/sort';
import { ExpenseService } from '../../../core/services/expense';
import { Expense } from '../../../core/models/expenses.model';
import { MaterialModule } from '../../../shared/material/material.module';

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

  dataSource = new MatTableDataSource<Expense>([]);
  loading = signal(false);
  error = signal('');
  pageSize= 0;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  private expenseService = inject(ExpenseService);
  categories = signal<{ id: string; name: string }[]>([]);
  private cdr = inject(ChangeDetectorRef);
  private searchText = '';
  private categoryFilter = '';

  ngOnInit(): void {
    this.loadExpenses();
    
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.filterPredicate = (data: Expense) => {
      const matchesSearch = !this.searchText || 
        data.title.toLowerCase().includes(this.searchText) ||
        data.description?.toLowerCase().includes(this.searchText) ||
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
        const unique = [...new Set(expenses.map(e => e.category))]
          .map(name => ({ id: name, name }));
        this.categories.set(unique);
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
  
  //TODO
  addExpense(){

  }

  //TODO
  editExpense(id: number){

  }

  //TODO
  deleteExpense(id: number){

  }


}
