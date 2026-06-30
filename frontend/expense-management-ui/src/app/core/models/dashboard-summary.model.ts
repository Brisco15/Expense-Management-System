export interface RecentExpenseItem {
    id: number;
    title: string;
    amount: number;
    category: string;
    status: string;
    expenseDate: string;
    createdBy: string;
}

export interface DashboardSummary {
    totalExpenses: number;
    totalExpensesThisMonth: number;
    totalExpensesThisYear: number;
    totalExpenseCount: number;
    approvedExpenses: number;
    pendingExpenses: number;
    rejectedExpenses: number;
    rejectedExpenseCount: number;
    recentExpenses: RecentExpenseItem[];
}
