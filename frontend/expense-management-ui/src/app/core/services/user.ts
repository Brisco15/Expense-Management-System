import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/user`;

  constructor(
    private http: HttpClient,
  ){}

  getAllUsers(){
    return this.http.get<User[]>(this.apiUrl);
  }

  getUser(id: number){
    return this.http.get<User>(`${this.apiUrl}/${id}`)
  }

  // updateUser(id: number, ){
  //   return this.http.put<User>(`${this.apiUrl}/${id}/role`)
  // }

  deleteUser(id: number){
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
  }
}
