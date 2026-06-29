import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { HttpClient } from '@angular/common/http';
import { Expense } from '../models/expenses.model';
import { CreateExpense } from '../models/create-expense.model';
import { ApproveExpense } from '../models/approve-expense.model';
import { EditExpense } from '../models/edit-expense.model';
import { PagedResult } from '../models/paged-result.model';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root',
})
export class ExpenseService {

  private apiUrl = `${environment.apiUrl}/expense`;

  constructor(private http: HttpClient) {}

  getAllExpenses(): Observable<Expense[]> {
    return this.http.get<PagedResult<Expense>>(this.apiUrl).pipe(
      map(response => response.items)
    );
  }

  getExpenseById(id: number) : Observable<Expense> {
    return this.http.get<Expense>(`${this.apiUrl}/${id}`);
  }

  getExpensesByUser(): Observable<Expense[]> {
    return this.http.get<Expense[]>(`${this.apiUrl}/user`);
  }

  create( newExpense: CreateExpense):Observable<Expense> {
    return this.http.post<Expense>(this.apiUrl, newExpense);
  }

  approve(id: number, approvedExpense: ApproveExpense){
    return this.http.post<Expense>(`${this.apiUrl}/${id}/approve`, approvedExpense);
  }

  update( id: number, updatedExpense: EditExpense): Observable<any> {
    return this.http.put<Expense>(`${this.apiUrl}/${id}`, updatedExpense);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

