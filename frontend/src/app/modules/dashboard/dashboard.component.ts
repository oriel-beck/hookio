import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { injectParams } from 'ngxtension/inject-params';
import { SubscriptionService } from '../../services/subscription/subscription.service';
import { Subscription, SubscriptionType } from '../../schemas/subscription.schema';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { BadgeModule } from 'primeng/badge';
import { Router, RouterModule } from '@angular/router';
import { DialogService } from 'primeng/dynamicdialog';
import { AddSubscriptionComponent } from '../../components/add-subscription/add-subscription.component';
import { ButtonModule } from 'primeng/button';

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
  imports: [
    CommonModule,
    CardModule,
    TableModule,
    BadgeModule,
    RouterModule,
    ButtonModule
  ],
  providers: [
    DialogService
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly dialogService = inject(DialogService);
  private readonly router = inject(Router)

  readonly subscriptions = signal<Subscription[]>([]);
  readonly recentActions = signal<RecentAction[]>([
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

  readonly totalSubscriptions = computed(() => this.subscriptions().length)
  readonly youtubeSubscriptions = computed(() => this.subscriptions().filter(s => s.subscriptionType === SubscriptionType.YouTube).length)
  readonly twitchSubscriptions = computed(() => this.subscriptions().filter(s => s.subscriptionType === SubscriptionType.Twitch).length)

  readonly params = injectParams();

  readonly guildId = computed<string>(() => this.params()['guildId']);

  public openAddSubscriptionDialog() {
    const dialog = this.dialogService.open(AddSubscriptionComponent, {
      showHeader: false,
      width: '600px',
      modal: true,
      data: {
        guildId: this.guildId()
      }
    });

    dialog.onClose.subscribe({
      next: (v?: Subscription) => {
        if (v) {
          this.router.navigate(["dashboard", this.guildId(), v.id]);
        }
      }
    })
  }

  ngOnInit(): void {
    this.subscriptionService.getSubscriptions(this.guildId()).subscribe({
      next: (v) => this.subscriptions.set(v)
    });
  }
}
