export interface Expense {
    id: number;
    title: string;
    amount: number;
    expenseDate: string;
    status: string;
    description?: string;
    category: string;
    user: string;
}