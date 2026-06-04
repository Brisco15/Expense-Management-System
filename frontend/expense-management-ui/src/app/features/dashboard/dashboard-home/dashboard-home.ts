import { Component, DestroyRef, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin } from 'rxjs';
import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartOptions } from 'chart.js';
import { DashboardSummary } from '../../../core/models/dashboard-summary.model';
import { MonthlyExpense } from '../../../core/models/monthly-expense.model';
import { CategoryExpense } from '../../../core/models/category-expense.model';
import { DashboardService } from '../../../core/services/dashboard';
import { MaterialModule } from '../../../shared/material/material.module';

@Component({
  selector: 'app-dashboard-home',
  imports: [MaterialModule, CommonModule, BaseChartDirective],
  templateUrl: './dashboard-home.html',
  styleUrl: './dashboard-home.css',
})
export class DashboardHome implements OnInit {
  summary = signal<DashboardSummary | null>(null);
  monthlyExpenses = signal<MonthlyExpense[]>([]);
  categoryExpenses = signal<CategoryExpense[]>([]);
  loading = signal(false);
  error = signal('');

  private dashboardService = inject(DashboardService);
  private destroyRef = inject(DestroyRef);

  barChartData = computed<ChartData<'bar'>>(() => ({
    labels: this.monthlyExpenses().map(e => e.month),
    datasets: [{
      label: 'Monthly Expenses (€)',
      data: this.monthlyExpenses().map(e => e.totalAmount),
      backgroundColor: 'rgba(99, 102, 241, 0.7)',
      borderColor: '#6366f1',
      borderWidth: 1,
      borderRadius: 6,
    }]
  }));

  pieChartData = computed<ChartData<'doughnut'>>(() => ({
    labels: this.categoryExpenses().map(e => e.category),
    datasets: [{
      data: this.categoryExpenses().map(e => e.totalAmount),
      backgroundColor: ['#6366f1','#06b6d4','#f59e0b','#10b981','#f43f5e','#8b5cf6'],
      hoverOffset: 6,
    }]
  }));

  barChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true } }
  };

  pieChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    plugins: { legend: { position: 'bottom' } }
  };

  ngOnInit(): void {
    this.loading.set(true);

    forkJoin({
      summary: this.dashboardService.getSummary(),
      monthly: this.dashboardService.getMonthlyExpenses(),
      category: this.dashboardService.getCategoryExpenses(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ summary, monthly, category }) => {
          this.summary.set(summary);
          this.monthlyExpenses.set(monthly);
          this.categoryExpenses.set(category);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Failed to load dashboard data.');
          this.loading.set(false);
        },
      });
  }
}

