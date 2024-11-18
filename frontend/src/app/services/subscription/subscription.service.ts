import { inject, Injectable } from '@angular/core';
import { HttpService } from '../http/http.service';
import { subscription, Subscription } from '../../schemas/subscription.schema';
import { map, Observable } from 'rxjs';
import { User } from '../../schemas/user.schema';

// validate that real subscriptions are returned and parse their data
const validateSubscription = (sub: Subscription) => subscription.parse(sub);
const validateSubscriptions = (subs: Subscription[]) => subs.map(validateSubscription);

type GuildId = User['guilds'][0]['id'] | string;

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private httpService = inject(HttpService);

  public getSubscriptions(guildId: GuildId, subscriptionId: number): Observable<Subscription>;
  public getSubscriptions(guildId: GuildId): Observable<Subscription[]>;
  public getSubscriptions(guildId: GuildId, subscriptionId?: number): Observable<Subscription | Subscription[]> {
    if (subscriptionId) return this.httpService.get<Subscription>(`/api/subscriptions/${guildId}/${subscriptionId}`);
    return this.httpService.get<Subscription[]>(`/api/subscriptions/${guildId}`).pipe(map(validateSubscriptions));
  }

  public createSubscription(guildId: GuildId, data: Omit<Subscription, 'id' | 'guildId' | 'messages'> & { webhookUrl: string }) {
    return this.httpService.post<Subscription>(`/api/subscriptions/${guildId}`, data).pipe(map(validateSubscription));
  }

  public patchSubscription(guildId: GuildId, subscriptionId: number, data: Partial<Subscription>) {
    return this.httpService.patch<Subscription>(`/api/subscriptions/${guildId}/${subscriptionId}`, data).pipe(map(validateSubscription));
  }

  public deletesubscription(guildId: GuildId, subscriptionId: number) {
    return this.httpService.delete<boolean>(`/api/subscriptions/${guildId}/${subscriptionId}`);
  }
}
