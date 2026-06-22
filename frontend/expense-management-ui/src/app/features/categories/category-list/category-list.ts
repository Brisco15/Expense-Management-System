import { Component, signal, AfterViewInit, OnInit, OnDestroy, ChangeDetectorRef, inject, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { CategoryExpense } from '../../../core/models/category-expense.model';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '../../../shared/material/material.module';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { CategoryService } from '../../../core/services/category';

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
    'actions'
  ];

  dataSource = new MatTableDataSource<CategoryExpense>([]);
  loading = signal(false);
  error = signal('');
  trackById = ( _index: number, category: CategoryExpense) => category.categoryId;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private categoryService = inject(CategoryService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit(): void {
    
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  ngOnDestroy(): void {
    
  }

  loadCategories(): void{
    
  }

  applyFilter(event: Event){

  }

  addCategory(){

  }

}
