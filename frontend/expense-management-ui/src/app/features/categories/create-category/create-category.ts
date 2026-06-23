import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MaterialModule } from '../../../shared/material/material.module';

@Component({
  selector: 'app-create-category',
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,

  ],
  templateUrl: './create-category.html',
  styleUrl: './create-category.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateCategory {
  form: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<CreateCategory>
  ){
    this.form = this.formBuilder.group({
      categoryName: ['', [Validators.maxLength(50), Validators.required]],
      description: ['', [Validators.maxLength(100), Validators.required]],
      monthlyBudget: [null, Validators.required],
      //yearlyBudget: [null, Validators.required],
      
    })
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
