import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environment';
import { LoginRequest } from '../models/login.model';
import { RegisterRequest } from '../models/register.model';

@Injectable({
  providedIn: 'root',
})
export class Auth {}
