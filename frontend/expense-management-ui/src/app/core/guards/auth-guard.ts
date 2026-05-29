import { CommonModule } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
 
  const isLogged = authService.isLoggedIn();

  if(!isLogged){
    router.navigateByUrl('/auth/login')
    return false
  }
  
  return true;
};
