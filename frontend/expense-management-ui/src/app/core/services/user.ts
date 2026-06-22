import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { User, UpdateUserRoleDto, UpdateUserStatusDto } from '../models/user.model';
import { PagedResult } from '../models/paged-result.model';


@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/user`;

  constructor(
    private http: HttpClient,
  ){}

  getAllUsers(): Observable<User[]> {
    return this.http.get<PagedResult<User>>(this.apiUrl).pipe(
      map(response => response.items)
    );
  }

  getUserById(id: number): Observable<User>{
    return this.http.get<User>(`${this.apiUrl}/${id}`)
  }

  updateUserRole(id: number, dto: UpdateUserRoleDto ): Observable<any>{
    return this.http.put<User>(`${this.apiUrl}/${id}/role`, dto)
  }

  updateUserStatus(id: number, dto: UpdateUserStatusDto): Observable<any>{
    return this.http.put<User>(`${this.apiUrl}/${id}/status`, dto)
  }

  deleteUser(id: number): Observable<void>{
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
  }
}
