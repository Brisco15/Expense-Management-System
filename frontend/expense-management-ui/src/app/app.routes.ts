import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { DashboardHome } from './features/dashboard/dashboard-home/dashboard-home';
import { MainLayout } from './core/layout/main-layout/main-layout';
import { UserManagement } from './features/users/user-management/user-management';
import { CategoryList } from './features/categories/category-list/category-list';
import { ExpenseList } from './features/expenses/expense-list/expense-list';

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
        path: '',
        component: MainLayout,
        canActivate: [authGuard],
        children: [
            {
                path: 'dashboard',
                component: DashboardHome
            },
            {
                path: 'users',
                component: UserManagement
            },
            {
                path: 'categories',
                component: CategoryList
            },
            {
                path: 'expenses',
                component: ExpenseList
            },

        ]
    }
];
