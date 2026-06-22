export interface UpdateCategory {
    categoryId: number;
    categoryName: string;
    description?: string;
    monthlyBudget?: number;
    yearlyBudget?: number;
    isActive: boolean
}