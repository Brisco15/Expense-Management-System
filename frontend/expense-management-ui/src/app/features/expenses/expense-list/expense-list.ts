import { CommonModule } from '@angular/common';
import { Component, AfterViewInit, OnInit, ViewChild, signal, inject } from '@angular/core';
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
    MatPaginator
],
  templateUrl: './expense-list.html',
  styleUrl: './expense-list.css',
})
export class ExpenseList implements OnInit { 

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

  ngOnInit(): void {
    this.loading.set(true);
    this.expenseService.getAllExpenses().subscribe({
      next: expenses => {
        this.dataSource.data = expenses;
        const unique = [...new Set(expenses.map(e => e.category))]
          .map(name => ({ id: name, name }));
        this.categories.set(unique);
        this.loading.set(false);
        this.error.set('');
      },
      error: ()=> {
        this.error.set('Failed to load Expense list');
        this.loading.set(false);
      }
    })
  }

  applyFilter(event: Event){
    const filterValue = (event.target as HTMLInputElement).value;

    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  filterByCategory(category: string) {
    if(!category){
      this.dataSource.filter = '';
      return;
    }

    this.dataSource.filterPredicate = (data, filter)=>
      data.category.toLocaleLowerCase().includes(filter);

    this.dataSource.filter = category.toLocaleLowerCase();
  }


}
