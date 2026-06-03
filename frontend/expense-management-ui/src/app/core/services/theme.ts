import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private darkModeSignal = signal(false);
  darkMode = this.darkModeSignal.asReadonly();

  constructor(){
    const savedTheme = localStorage.getItem('theme');
    if(savedTheme === 'dark'){
      this.enableDarkMode();
    } 
  }

  toggleTheme(){
    this.darkMode() ? this.enableLightMode() : this.enableDarkMode();
     
  }

  private enableDarkMode(){
    document.body.classList.add('dark-theme');
    localStorage.setItem('theme', 'dark');
    this.darkModeSignal.set(true);
  }

  private enableLightMode(){
    document.body.classList.remove('dark-theme');
    localStorage.setItem('theme', 'light' );
    this.darkModeSignal.set(false);
  }
}
