import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateExpenseStatus } from './update-expense-status';

describe('UpdateExpenseStatus', () => {
  let component: UpdateExpenseStatus;
  let fixture: ComponentFixture<UpdateExpenseStatus>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateExpenseStatus],
    }).compileComponents();

    fixture = TestBed.createComponent(UpdateExpenseStatus);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
