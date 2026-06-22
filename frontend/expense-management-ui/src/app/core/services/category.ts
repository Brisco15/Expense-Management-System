import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PagedResult } from '../models/paged-result.model';
import { CategoryExpense } from '../models/category-expense.model';
import { HttpClient } from '@angular/common/http';
import { CreateCategory } from '../models/create-category.model';
import { UpdateCategory } from '../models/update-category.model';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {

  private apiUrl = `${environment.apiUrl}/category`;

  constructor( private http: HttpClient){};

  getAllCategories(): Observable<CategoryExpense[]>{
    return this.http.get<PagedResult<CategoryExpense>>(this.apiUrl).pipe(
      map(response => response.items)
    )
  }

  getCategoryById(id: number): Observable<CategoryExpense>{
    return this.http.get<CategoryExpense>(this.apiUrl)
  }

  createCategory(dto: CreateCategory ): Observable<CategoryExpense>{
    return this.http.post<CategoryExpense>(this.apiUrl, dto)
  }

  updateCategory(id: number, dto: UpdateCategory): Observable<any>{
    return this.http.put<CategoryExpense>(`${this.apiUrl}/${id}`, dto)
  }

  softDelete(id: number): Observable<any>{
    return this.http.patch<CategoryExpense>(`${this.apiUrl}/${id}/deactivate`, true)
  }

  deleteCategory(id: number): Observable<void>{
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
  }

}
