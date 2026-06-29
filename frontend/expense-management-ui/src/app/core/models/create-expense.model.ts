export interface CreateExpense {
    title: string;
    amount: number;
    expenseDate: string;
    description?: string;
    categoryId: number;
    
}
