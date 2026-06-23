export interface CreateCategory {
    categoryName: string;
    description?: string;
    monthlyBudget?: number;
    yearlyBudget?: number;
    totalExpenses: number;
    totalAmount: number;
    createdAt: string;
    isActive: boolean;
}