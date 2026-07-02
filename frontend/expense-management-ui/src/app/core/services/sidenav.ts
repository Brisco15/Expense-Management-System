import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SidenavService {
  readonly isOpen = signal(true);

  toggle(): void { this.isOpen.update(open => !open); }
  open(): void   { this.isOpen.set(true); }
  close(): void  { this.isOpen.set(false); }
}
