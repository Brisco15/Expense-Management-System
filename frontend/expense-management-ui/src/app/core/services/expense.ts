import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { HttpClient } from '@angular/common/http';
import { Expense } from '../models/expenses.model';
import { CreateExpense } from '../models/create-expense.model';

@Injectable({
  providedIn: 'root',
})
export class ExpenseService {

  private apiUrl = `${environment.apiUrl}/expense`;

  constructor(private http: HttpClient) {}

  getAllExpenses() {
    return this.http.get<Expense[]>(this.apiUrl);
  }

  getExpenseById(id: number) {
    return this.http.get<Expense>(`${this.apiUrl}/${id}`);
  }

  getExpensesByUser() {
    return this.http.get<Expense[]>(`${this.apiUrl}/user`);
  }

  create(dto: CreateExpense) {
    return this.http.post<Expense>(this.apiUrl, dto);
  }

  update(dto: Expense) {
    return this.http.put<Expense>(`${this.apiUrl}/user`, dto);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

