import { Component, computed } from '@angular/core';
import { RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth';
import { MaterialModule } from '../../../shared/material/material.module';

@Component({
  selector: 'app-sidebar',
  imports: [
    
    MaterialModule,
    RouterLinkActive
],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {

  constructor(
    public authService: AuthService
  ){}

  isAdmin = computed(()=> this.authService.isAdmin())
}
