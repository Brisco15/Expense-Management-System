import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from '../../../shared/material/material.module';
import { CategoryExpense } from '../../../core/models/category-expense.model';

function atLeastOneBudget(group: AbstractControl): ValidationErrors | null {
  const monthly = group.get('monthlyBudget')?.value;
  const yearly  = group.get('yearlyBudget')?.value;
  return (monthly == null && yearly == null) ? { atLeastOne: true } : null;
}

@Component({
  selector: 'app-update-category-budget',
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './update-category-budget.html',
  styleUrl: './update-category-budget.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateCategoryBudget {
  form!: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<UpdateCategoryBudget>,
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
      monthlyBudget: [this.data.category.monthlyBudget ?? null, [Validators.min(0)]],
      yearlyBudget:  [this.data.category.yearlyBudget  ?? null, [Validators.min(0)]],
    }, { validators: atLeastOneBudget })
  }

  onCancel(): void{
    this.dialogRef.close();
  }

  onSave(): void{
    if(!this.form.valid){
      this.form.markAllAsTouched();
      return;
    };

    const formValue = this.form.value;

    const result : CategoryExpense = {
      categoryId: this.data.category.categoryId,
      categoryName: this.data.category.categoryName,
      description: this.data.category.description,
      monthlyBudget: formValue.monthlyBudget,
      yearlyBudget: formValue.yearlyBudget,
      totalExpenses: this.data.category.totalExpenses,
      totalAmount: this.data.category.totalAmount,
      updatedAt: new Date().toISOString(),
      isActive: true,
      createdAt: this.data.category.createdAt
    };

    this.dialogRef.close(result);
  }


}
