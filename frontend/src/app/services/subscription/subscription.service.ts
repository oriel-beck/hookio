import { inject, Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { Subscription } from '../../schemas/subscription.schema';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private httpService = inject(HttpService);

  public getSubscriptions(guildId: bigint, subscriptionId: number): Observable<Subscription>;
  public getSubscriptions(guildId: bigint): Observable<Subscription[]>;
  public getSubscriptions(guildId: bigint, subscriptionId?: number): Observable<Subscription | Subscription[]> {
    if (subscriptionId) return this.httpService.get<Subscription>(`/api/subscriptions/${guildId}/${subscriptionId}`);
    return this.httpService.get<Subscription[]>(`/api/subscriptions/${guildId}`);
  }

  public createSubscription(guildId: bigint, data: Omit<Subscription, 'id'>) {
    return this.httpService.post<Subscription | null>(`/api/subscriptions/${guildId}`, data);
  }

  public patchSubscription(guildId: bigint, subscriptionId: number, data: Partial<Subscription>) {
    return this.httpService.patch<Subscription | null>(`/api/subscriptions/${guildId}/${subscriptionId}`, data);
  }

  public deletesubscription(guildId: bigint, subscriptionId: number) {
    return this.httpService.delete<boolean>(`/api/subscriptions/${guildId}/${subscriptionId}`);
  }
}
