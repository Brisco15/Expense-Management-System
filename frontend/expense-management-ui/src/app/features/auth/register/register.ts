import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, ValidationErrors, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterModule, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { MaterialModule } from '../../../shared/material/material.module';


function passwordMatchValidator(
  control: AbstractControl
): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, CommonModule, MaterialModule, RouterModule, RouterLinkActive],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  loading = signal(false);
  error = signal('');
  hidePassword = signal(true);
  hideConfirmPassword = signal(true);
  registerForm;
  
  
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    
  ){
    this.registerForm = this.fb.group({
      fullName: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100)
        ]
      ],
      email: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[^@]+@[^@]+\.[^\s@]+$/),
          Validators.email

        ]
      ],
      
      password: [
        '',
        [
          Validators.required,
          Validators.pattern('^(?=.*[A-Z])(?=.*[0-9])(?=.*[a-z])(?=.*[!#$%&?€]).{8,}$')
        ]
      ],
      confirmPassword: [
        '',
        Validators.required
      ]
    },
  {
    validators: passwordMatchValidator
  })
  }

  register(){

    if (this.registerForm.invalid){
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set('');

    
     this.authService.register({
      fullName: this.registerForm.value.fullName!,
      email: this.registerForm.value.email!,
      password: this.registerForm.value.password!,
      confirmPassword: this.registerForm.value.confirmPassword!
     }).subscribe({
      next: (response) => {
        this.authService.saveUser(response);
        this.loading.set(false);
        this.router.navigateByUrl('auth/login');
      },
      error: () => {
        
          this.loading.set(false);
          this.error.set('Registration failed. try again')
        
      }
     })
  }

}
