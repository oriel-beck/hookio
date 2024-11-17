import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { UserService } from '../../services/user/user.service';
import { catchError, map, of } from 'rxjs';

export const loggedInGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  return userService.getCurrentUser().pipe(
    map(v => !!v),
    catchError(() => of(false))
  );
};
