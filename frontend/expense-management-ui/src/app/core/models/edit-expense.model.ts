export interface EditExpense{
    id: number,
    title: string,
    amount: number,
    description?: string,
    categoryId: number,
    updatedAt: string
}