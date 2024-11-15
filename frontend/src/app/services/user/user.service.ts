import { inject, Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly httpService = inject(HttpService);

  public getCurrentUser() {
    return this.httpService.get<unknown>(`/api/users/getcurrentuser`);
  }
}
