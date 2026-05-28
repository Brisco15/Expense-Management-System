export interface Expense {
    id: number;
    title: string;
    amount: number;
    expenseDate: Date;
    status: string;
    description?: string;
    category: string;
    user: string;
}