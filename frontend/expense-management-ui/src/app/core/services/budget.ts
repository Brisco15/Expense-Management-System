import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment';
import { CategoryExpense } from '../models/category-expense.model';
import { UpdateCategoryBudget } from '../models/update-category-budget.model';



@Injectable({
  providedIn: 'root',
})
export class BudgetService {
  private apiUrl = `${environment.apiUrl}/budget`;
  
  constructor(private http: HttpClient){};

  updateCategoryBudget(id: number, updatedCategoryBudget: UpdateCategoryBudget): Observable<any>{
    return this.http.put<CategoryExpense>(`${this.apiUrl}/category/${id}`, updatedCategoryBudget)
  }

  getCategoryBudgetById(id: number): Observable<CategoryExpense>{
    return this.http.get<CategoryExpense>(`${this.apiUrl}/category/${id}`)
  }


}
