import { inject, Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { User, userData } from '../../schemas/user.schema';
import { map } from 'rxjs';

const validateUser = (user: User) => userData.parse(user);

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly httpService = inject(HttpService);

  public getCurrentUser() {
    return this.httpService.get<User>(`/api/users/getcurrentuser`).pipe(
      map(validateUser)
    );
  }
}
