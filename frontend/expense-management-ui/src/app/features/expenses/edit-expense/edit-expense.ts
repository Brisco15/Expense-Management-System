import { Component, ChangeDetectionStrategy, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from '../../../shared/material/material.module';
import { Expense } from '../../../core/models/expenses.model';
import { CategoryExpense } from '../../../core/models/category-expense.model';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-edit-expense',
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './edit-expense.html',
  styleUrl: './edit-expense.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditExpense {
  form!: FormGroup;
  selectedFile: File | null = null;
  selectedFileName = '';
  readonly allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];

  constructor(
    private dialogRef: MatDialogRef<EditExpense>,
    private formBuilder: FormBuilder,
    private authService: AuthService,
    @Inject(MAT_DIALOG_DATA) public data: { categories: CategoryExpense[],
      expense: Expense
    }
  ){
    this.initializeForm();
  }

  private initializeForm():void{
    if(!this.data?.expense){
      this.dialogRef.close();
      return;
    }
    this.form = this.formBuilder.group({
      title: [this.data.expense.title || '',[Validators.maxLength(50), Validators.required] ],
      description: [this.data.expense.description || '', [Validators.maxLength(100)]],
      amount: [this.data.expense.amount || null, [Validators.required, Validators.min(0.01)]],
      categoryId: [this.data.expense.categoryId ?? null, [Validators.required]] 
    })
  }

  get categories(): CategoryExpense[] {
    return this.data?.categories ?? [];
  }

  onCancel(): void{
    this.dialogRef.close();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    if (!file) return;

    if (!this.allowedTypes.includes(file.type)) {
      input.value = '';
      this.selectedFile = null;
      this.selectedFileName = 'Invalid type. Only JPG, PNG, PDF allowed.';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      input.value = '';
      this.selectedFile = null;
      this.selectedFileName = 'File exceeds 5MB limit.';
      return;
    }
    this.selectedFile = file;
    this.selectedFileName = file.name;
  }

  onSave(): void {
    if(!this.form.valid){
      this.form.markAllAsTouched();
      return;
    };

    const formValue = this.form.value;

    const result = {
      id: this.data.expense.id,
      title: formValue.title,
      description: formValue.description,
      amount: formValue.amount,
      categoryId: formValue.categoryId,
      expenseDate: this.data.expense.expenseDate,
      status: this.data.expense.status,
      createdBy: this.data.expense.createdBy,
      category: this.data.expense.category,
      receiptFile: this.selectedFile
    };

    this.dialogRef.close(result)

  }
}
