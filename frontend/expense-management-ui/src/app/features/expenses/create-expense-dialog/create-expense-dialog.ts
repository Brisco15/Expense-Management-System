import { Component, ChangeDetectionStrategy, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from '../../../shared/material/material.module';
import { CategoryExpense } from '../../../core/models/category-expense.model';

@Component({
  selector: 'app-create-expense-dialog',
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './create-expense-dialog.html',
  styleUrl: './create-expense-dialog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateExpenseDialog {
  form: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<CreateExpenseDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { categories: CategoryExpense[] }
  ){
    this.form = this.formBuilder.group({
      title: ['', [Validators.required, Validators.maxLength(50)]],
      description: ['', [Validators.maxLength(100)]],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      categoryId: [null, [Validators.required]]
    })
  }

  get categories(): CategoryExpense[] {
    return this.data?.categories ?? [];
  }

  onCreate(){
    if(this.form.valid){
      this.dialogRef.close(this.form.value)
    }
  }

  onCancel(){
    this.dialogRef.close()
  }
  
}
