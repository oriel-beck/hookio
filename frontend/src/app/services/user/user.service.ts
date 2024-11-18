import { inject, Injectable, signal } from '@angular/core';
import { HttpService } from '../http/http.service';
import { User, userData } from '../../schemas/user.schema';
import { catchError, finalize, map, Observable, of, shareReplay, tap, throwError } from 'rxjs';
import { CookieService } from 'ngx-cookie-service';

const validateUser = (user: User) => userData.parse(user);

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly httpService = inject(HttpService);
  private readonly cookieService = inject(CookieService);

  readonly user = signal<User | undefined>(undefined);
  private fetchingUser$?: Observable<User>; // Cache pending request

  constructor() {
    // Eagerly fetch the user once and populate the signal
    this.getCurrentUser().subscribe({
      next: (user) => this.user.set(user),
    });
  }

  private fetchCurrentUser(): Observable<User> {
    return this.httpService.get<User>(`/api/users/getcurrentuser`).pipe(
      map(validateUser),
      shareReplay(1) // Cache the HTTP result
    );
  }

  public getCurrentUser(): Observable<User | undefined> {
    if (this.user()) {
      // Return cached user as an observable
      return of(this.user());
    }

    // If a fetch is already in progress, return the same observable
    if (!this.fetchingUser$) {
      this.fetchingUser$ = this.fetchCurrentUser().pipe(
        tap((user) => this.user.set(user)), // Update signal
        finalize(() => (this.fetchingUser$ = undefined)), // Clear after completion
        catchError((err) => {
          // TODO: test this when the session expires (there seems to be a login issue)
          this.cookieService.delete(".AspNetCore.Cookies");
          this.cookieService.delete(".Hookio.Session");
          return throwError(() => err);
        })
      );
    }

    return this.fetchingUser$;
  }
}

