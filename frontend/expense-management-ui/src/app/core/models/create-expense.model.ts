export interface CreateExpense {
    title: string;
    amount: number;
    expenseDate: Date;
    description?: string;
    categoryId: number;
    categoryName: string;
}
