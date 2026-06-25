import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateCategoryBudget } from './update-category-budget';

describe('UpdateCategoryBudget', () => {
  let component: UpdateCategoryBudget;
  let fixture: ComponentFixture<UpdateCategoryBudget>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateCategoryBudget],
    }).compileComponents();

    fixture = TestBed.createComponent(UpdateCategoryBudget);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
