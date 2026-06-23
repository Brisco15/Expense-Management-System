import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environment';
import { LoginRequest } from '../models/login.model';
import { RegisterRequest } from '../models/register.model';
import { AuthResponse } from '../models/auth-response.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/auth`;

  private userSignal = signal<AuthResponse | null>(null);
  user = this.userSignal.asReadonly();
 
  isLoggedIn = computed(() => !!this.user());

  isAdmin = computed(()=> this.user()?.role === 'Admin');

  constructor(private http: HttpClient){
    this.loadUserFromStorage();
  }

  login(data: LoginRequest){
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data);
  }

  register(data: RegisterRequest){
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data)
  }

  saveUser(user: AuthResponse){
    localStorage.setItem('user', JSON.stringify(user));
    this.userSignal.set(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.userSignal.set(null);
  }

  isTokenExpired(): boolean {
    const user = this.getUser();
    if (!user?.expiresAt) return true;
    return new Date(user.expiresAt) <= new Date();
  }

  getToken(): string | null {
    if (this.isTokenExpired()) {
      this.logout();
      return null;
    }
    const user = this.getUser();
    return user ? user.token : null;
  }

  getUser(): AuthResponse | null {
    const data = localStorage.getItem('user');
    return data ? JSON.parse(data) : null;
  }

  private loadUserFromStorage(){
    const user = this.getUser();
    if(user){
      this.userSignal.set(user);
    }
  }
}
