export interface Expense {
    id: number;
    title: string;
    amount: number;
    expenseDate: string;
    status: string;
    description?: string;
    category: string;
    categoryId: number;
    createdBy: string;
    hasReceipt?: boolean;
    receiptFileName?: string;
}