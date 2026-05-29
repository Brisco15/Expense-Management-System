import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { DashboardHome } from './features/dashboard/dashboard-home/dashboard-home';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'auth/login',
        pathMatch: 'full'

    },
    {
        path: 'auth/login',
        component: Login
    },
    {
        path: 'auth/register',
        component: Register
    },
    {
        path: 'dashboard',
        component: DashboardHome,
        canActivate: [authGuard]
    }
];
