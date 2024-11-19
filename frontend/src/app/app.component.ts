import { Component, inject, OnInit, signal } from '@angular/core';
import { NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router, RouterEvent, RouterOutlet } from '@angular/router';
import { LayoutComponent } from "./components/layout/layout.component";
import { UserService } from './services/user/user.service';
import { LoadingOverlayComponent } from "./components/loading-overlay/loading-overlay.component";

@Component({
  selector: 'hookio-root',
  standalone: true,
  imports: [RouterOutlet, LayoutComponent, LoadingOverlayComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  readonly userService = inject(UserService);
  private readonly router = inject(Router)

  navigating = signal(false);

  constructor() {
    this.router.events.subscribe({
      next: (ev) => {
        if (ev instanceof NavigationStart) {
          this.navigating.set(true)
        }
        if (ev instanceof NavigationEnd) {
          this.navigating.set(false);
        }

        // Set loading state to false in both of the below events to hide the spinner in case a request fails
        if (ev instanceof NavigationCancel) {
          this.navigating.set(false);
        }
        if (ev instanceof NavigationError) {
          this.navigating.set(false);
        }
      }
    })
  }
}
