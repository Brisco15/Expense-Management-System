import { Component, ChangeDetectionStrategy, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from '../../../shared/material/material.module';
import { Expense } from '../../../core/models/expenses.model';


@Component({
  selector: 'app-update-expense-status',
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './update-expense-status.html',
  styleUrl: './update-expense-status.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UpdateExpenseStatus {
  form!: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<UpdateExpenseStatus>,
    private formBuilder: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: { expense: Expense }
  ){
    this.initializeForm();
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      isApproved: [true, [Validators.required]],
      rejectionReason: ['']
    });

    this.form.get('isApproved')?.valueChanges.subscribe((isApproved: boolean) => {
      const reasonCtrl = this.form.get('rejectionReason');
      if (!isApproved) {
        reasonCtrl?.setValidators([Validators.required, Validators.maxLength(500)]);
      } else {
        reasonCtrl?.clearValidators();
        reasonCtrl?.setValue('');
      }
      reasonCtrl?.updateValueAndValidity();
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.value);
  }
}

