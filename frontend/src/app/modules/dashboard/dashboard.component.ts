import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { injectParams } from 'ngxtension/inject-params';
import { SubscriptionService } from '../../services/subscription/subscription.service';
import { Subscription, SubscriptionType } from '../../schemas/subscription.schema';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { BadgeModule } from 'primeng/badge';
import { RouterModule } from '@angular/router';

interface RecentAction {
  platform: string;
  action: string;
  timestamp: string;
  status: string;
  source: string;
  subscriptionId: number;
}

@Component({
  selector: 'hookio-dashboard',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, BadgeModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  subscriptionService = inject(SubscriptionService);
  subscriptions = signal<Subscription[]>([]);
  recentActions = signal<RecentAction[]>([
    {
      platform: 'YouTube',
      action: 'SendMessage',
      timestamp: '21-01-2024',
      status: 'Success',
      source: 'Kripparian',
      subscriptionId: 1
    },
    {
      platform: 'Twitch',
      action: 'SendMessage',
      timestamp: '21-01-2024',
      status: 'Success',
      source: 'Baumi',
      subscriptionId: 2
    },
    {
      platform: 'Twitch',
      action: 'UpdateMessage',
      timestamp: '21-01-2024',
      status: 'Success',
      source: 'Baumi',
      subscriptionId: 2
    },
    {
      platform: 'YouTube',
      action: 'DeleteMessage',
      timestamp: '21-01-2024',
      status: 'Failed',
      source: 'Kripparian',
      subscriptionId: 1
    },
    {
      platform: 'Twitch',
      action: 'SendMessage',
      timestamp: '21-01-2024',
      status: 'Failed',
      source: 'Baumi',
      subscriptionId: 2
    }
  ]);

  totalSubscriptions = computed(() => this.subscriptions().length)
  youtubeSubscriptions = computed(() => this.subscriptions().filter(s => s.subscriptionType === SubscriptionType.YouTube).length)
  twitchSubscriptions = computed(() => this.subscriptions().filter(s => s.subscriptionType === SubscriptionType.Twitch).length)

  params = injectParams();

  guildId = computed<string>(() => this.params()['guildId']);

  ngOnInit(): void {
    this.subscriptionService.getSubscriptions(this.guildId()).subscribe({
      next: (v) => this.subscriptions.set(v)
    });
  }
}
