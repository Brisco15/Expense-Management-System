export interface ApproveExpense{
    expenseId: number;
    isApproved: boolean;
    rejectionReason?: string;
}