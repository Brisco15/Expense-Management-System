import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth';
import { MaterialModule } from '../../../shared/material/material.module';

@Component({
  selector: 'app-sidebar',
  imports: [
    MaterialModule,
    RouterLink,
    RouterLinkActive
],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {

  private authService = inject(AuthService);

  isAdmin = this.authService.isAdmin;
}
