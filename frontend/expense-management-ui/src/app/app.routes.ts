 import { Routes } from '@angular/router';
 import { authGuard } from './core/guards/auth-guard';
// import { Login } from './features/auth/login/login';
// import { Register } from './features/auth/register/register';
// import { DashboardHome } from './features/dashboard/dashboard-home/dashboard-home';
// import { MainLayout } from './core/layout/main-layout/main-layout';
// import { UserManagement } from './features/users/user-management/user-management';
// import { CategoryList } from './features/categories/category-list/category-list';
// import { ExpenseList } from './features/expenses/expense-list/expense-list';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'auth/login',
        pathMatch: 'full'

    },
    {
        path: 'auth/login',
        loadComponent:()=> import('./features/auth/login/login').then(m => m.Login)
    },
    {
        path: 'auth/register',
        loadComponent: ()=> import('./features/auth/register/register').then(m => m.Register)
    },
    {
        path: '',
        loadComponent: ()=> import('./core/layout/main-layout/main-layout').then(m => m.MainLayout),
        canActivate: [authGuard],
        children: [
            {
                path: 'dashboard',
                loadComponent: ()=> import('./features/dashboard/dashboard-home/dashboard-home').then(m => m.DashboardHome)
            },
            {
                path: 'users',
                loadComponent: ()=> import('./features/users/user-management/user-management').then(m => m.UserManagement)
            },
            {
                path: 'categories',
                loadComponent: ()=> import('./features/categories/category-list/category-list').then(m => m.CategoryList)
            },
            {
                path: 'expenses',
                loadComponent: ()=> import('./features/expenses/expense-list/expense-list').then(m => m.ExpenseList)
            },

        ]
    }
];
