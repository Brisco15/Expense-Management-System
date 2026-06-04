import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { HttpClient } from '@angular/common/http';
import { DashboardSummary } from '../models/dashboard-summary.model';
import { CategoryExpense } from '../models/category-expense.model';
import { MonthlyExpense } from '../models/monthly-expense.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {

  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient){}

  getSummary(){
    return this.http.get<DashboardSummary>(
      `${this.apiUrl}/summary`
    )
  }

  getMonthlyExpenses(){
    return this.http.get<MonthlyExpense[]>(
      `${this.apiUrl}/monthly-expenses`
    )
  }

  getCategoryExpenses(){
    return this.http.get<CategoryExpense[]>(
      `${this.apiUrl}/category-expenses`
    )
  }
}
