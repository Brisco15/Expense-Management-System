import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MaterialModule } from '../../../shared/material/material.module';
import { AuthService } from '../../services/auth';
import { ThemeService } from '../../services/theme';
import { SidenavService } from '../../services/sidenav';


@Component({
  selector: 'app-navbar',
  imports: [MaterialModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {

  public sidenavService = inject(SidenavService);

  constructor(
    public authService: AuthService,
    private router: Router,
    public themeService: ThemeService
  ){}
  
  logout(){
    this.authService.logout();
    this.router.navigateByUrl('/auth/login');
  }
}
