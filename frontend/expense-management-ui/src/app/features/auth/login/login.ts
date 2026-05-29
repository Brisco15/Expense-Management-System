import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { MaterialModule } from '../../../shared/material/material.module';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, MaterialModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  loading = signal(false);
  error = signal('');
  hidePassword = signal(true);
  loginForm;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ){
    this.loginForm = this.fb.group({
      email: [
        '',
        [
          Validators.required,
          Validators.email,
          Validators.pattern("/^[^@]+@[^@]+\.[^\s@]+$/")
        ]
      ],

      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
        ]
      ]
    });
  }

  login(){
    if(this.loginForm.invalid){
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    this.error.set('');

    this.authService.login({
      email: this.loginForm.value.email!,
      password: this.loginForm.value.password!
    })
    .subscribe({
      next: (response)=> {
        this.authService.saveUser(response);
        this.loading.set(false);
        this.router.navigateByUrl('/dashboard');
      },

      error: () => {
        this.loading.set(false);
        this.error.set('Invalid email or password')
      }
    })
  }
  
}
