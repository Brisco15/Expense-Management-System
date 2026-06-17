export interface CategoryExpense {
    categoryId: number;
    categoryName: string;
    description?: string;
    monthlyBudget: number;
    yearlyBudget?: number;
    isActive: boolean;
    createdAt:string;
    updatedAt: string; 
}