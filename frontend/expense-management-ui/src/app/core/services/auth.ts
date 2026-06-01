import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environment';
import { LoginRequest } from '../models/login.model';
import { RegisterRequest } from '../models/register.model';
import { AuthResponse } from '../models/auth-response.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/auth`;

  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient){
    this.loadUserFromStorage();
  }

  login(data: LoginRequest){
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data)
  }

  register(data: RegisterRequest){
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data)
  }

  

  saveUser(user: AuthResponse){
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null{
    const user = this.getUser();
    return user ? user.token : null;

  }

  getUser(): AuthResponse | null {
    const data = localStorage.getItem('user');
    return data ? JSON.parse(data) : null;
  }

  isLoggedIn(): boolean{
    return !!this.getToken();
  }

  private loadUserFromStorage(){
    const user = this.getUser();
    if(user){
      this.currentUserSubject.next(user);
    }
  }
}
