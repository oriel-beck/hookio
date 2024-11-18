import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { SubscriptionService } from '../../services/subscription/subscription.service';
import { Router } from '@angular/router';
import { Subscription } from '../../schemas/subscription.schema';
import { injectParams } from 'ngxtension/inject-params';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'hookio-editor',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './editor.component.html',
  styleUrl: './editor.component.scss'
})
export class EditorComponent implements OnInit {
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly router = inject(Router);
  private params = injectParams();

  readonly guildId = computed<string>(() => this.params()['guildId']);
  readonly subscriptionId = computed<string>(() => this.params()['subscriptionId']);

  readonly subscription = signal<Subscription | undefined>(undefined);
  readonly loading = computed(() => !this.subscription());

  ngOnInit(): void {
    const subscriptionAsNumber = Number(this.subscriptionId());
    if (isNaN(subscriptionAsNumber)) this.router.navigate(["servers", this.guildId()]);
    else this.subscriptionService.getSubscriptions(this.guildId(), subscriptionAsNumber).subscribe({
      next: (v) => this.subscription.set(v),
      error: (e: HttpErrorResponse) => {
        if (e.status === 403) this.router.navigate(["servers", this.guildId()]);
      }
    });
  }
}
