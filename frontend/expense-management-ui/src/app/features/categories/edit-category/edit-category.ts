import { CommonModule } from '@angular/common';
import { Component, ChangeDetectionStrategy, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule,Validators, FormControl } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from '../../../shared/material/material.module';
import { CategoryExpense } from '../../../core/models/category-expense.model';

@Component({
  selector: 'app-edit-category',
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './edit-category.html',
  styleUrl: './edit-category.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditCategory {
  form!: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<EditCategory>,
    private formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: {category: CategoryExpense}
  ){
    this.initializeForm();
  }

  private initializeForm(): void {
    if(!this.data?.category){
      this.dialogRef.close();
      return;
    }
    this.form = this.formBuilder.group({
      categoryName: [this.data.category.categoryName  || '', [Validators.maxLength(50), Validators.required]],
      description: [this.data.category.description || '', [Validators.maxLength(100), Validators.required]]
    })
  }

  

  onSave(): void{
    if(!this.form.valid){
      this.form.markAllAsTouched();
      return;
    };

    const formValue = this.form.value;

    const result : CategoryExpense = {
      categoryId: this.data.category.categoryId,
      categoryName: formValue.categoryName,
      description: formValue.description,
      monthlyBudget: this.data.category.monthlyBudget,
      yearlyBudget: this.data.category.yearlyBudget,
      totalExpenses: this.data.category.totalExpenses,
      totalAmount: this.data.category.totalAmount,
      updatedAt: new Date().toISOString(),
      isActive: true,
      createdAt: this.data.category.createdAt
    };

    this.dialogRef.close(result);
  }

  onCancel(): void{
    this.dialogRef.close();
  }


}
