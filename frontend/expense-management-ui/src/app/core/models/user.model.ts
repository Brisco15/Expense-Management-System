
export type UserRole = 'Admin' | 'Manager' | 'Employee';
export interface User {
    id: number;
    fullName: string;
    email: string;
    role: UserRole;
    isActive: boolean;
    createdAt: string;
    updatedAt: string;

}