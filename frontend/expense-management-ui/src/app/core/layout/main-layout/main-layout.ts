import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from '../navbar/navbar';
import { Sidebar } from '../sidebar/sidebar';
import { MaterialModule } from '../../../shared/material/material.module';
import { SidenavService } from '../../services/sidenav';
import { BreakpointObserver } from '@angular/cdk/layout';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    Navbar,
    Sidebar,
    MaterialModule
  ],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
})
export class MainLayout {
  protected sidenavService = inject(SidenavService);
  protected isOverlay = signal(false);

  constructor() {
    inject(BreakpointObserver)
      .observe('(max-width: 1280px)')
      .pipe(takeUntilDestroyed())
      .subscribe(result => {
        if (result.matches) {
          this.isOverlay.set(true);
          this.sidenavService.close();
        } else {
          this.isOverlay.set(false);
          this.sidenavService.open();
        }
      });
  }
}
