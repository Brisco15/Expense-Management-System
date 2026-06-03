import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from '../navbar/navbar';
import { Sidebar } from '../sidebar/sidebar';
import { MaterialModule } from '../../../shared/material/material.module';


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
export class MainLayout {}
