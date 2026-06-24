export interface CategoryExpense {
    categoryId: number;
    categoryName: string;
    description?: string;
    monthlyBudget: number;
    yearlyBudget?: number;
    totalExpenses: number;
    totalAmount: number;
    lastExpenseDate?: string;
    isActive: boolean;
    createdAt:string;
    updatedAt: string; 
}

export interface UpdateCategoryStatusDto {
    isActive: boolean;
}