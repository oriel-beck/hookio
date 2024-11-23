import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { DropdownModule } from 'primeng/dropdown';
import { SubscriptionType } from '../../schemas/subscription.schema';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { webhookRegex } from '../../constants';
import { HttpClient } from '@angular/common/http';
import { catchError, debounceTime, filter, of, switchMap, tap } from 'rxjs';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { SubscriptionService } from '../../services/subscription/subscription.service';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

type State = 'form' | 'loading' | 'error';

@Component({
  selector: 'hookio-add-subscription',
  standalone: true,
  imports: [
    DropdownModule,
    ReactiveFormsModule,
    InputTextModule,
    ButtonModule,
    InputGroupAddonModule,
    InputGroupModule,
    ProgressSpinnerModule
  ],
  templateUrl: './add-subscription.component.html',
  styleUrl: './add-subscription.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddSubscriptionComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly httpClient = inject(HttpClient);
  private readonly subscriptionService = inject(SubscriptionService);

  readonly config: DynamicDialogConfig<{ guildId: string }> = inject(DynamicDialogConfig)
  readonly ref = inject(DynamicDialogRef);

  loadingWebhook = signal(false);
  state = signal<State>('form');

  form = this.fb.group({
    subscriptionType: SubscriptionType.YouTube,
    webhookUrl: ["", [Validators.required, Validators.pattern(webhookRegex)]],
    // TODO: add custom validator based on selected subscription type
    source: ["", Validators.required]
  });

  platforms = [
    {
      label: 'YouTube',
      value: SubscriptionType.YouTube
    },
    {
      label: 'Twitch',
      value: SubscriptionType.Twitch
    }
  ]

  ngOnInit(): void {
    this.form.get('webhookUrl')?.valueChanges
      .pipe(
        filter((v) => !!v && webhookRegex.test(v || "")),
        tap(() => this.loadingWebhook.set(true)),
        debounceTime(1000),
        switchMap((url) => this.httpClient.get(url!).pipe(catchError(() => of(null)))),
      ).subscribe({
        next: (v) => {
          const webhookControl = this.form.get('webhookUrl')!
          if (!v) webhookControl.setErrors({ notFound: "Webhook not found" });
          else webhookControl.setErrors(null);
          this.loadingWebhook.set(false);
        }
      });
  }

  close(confirmed = false) {
    if (!confirmed) return this.ref.close();
    if (this.form.invalid) return;

    this.state.set('loading');
    this.subscriptionService.createSubscription(this.config.data!.guildId, {
      subscriptionType: this.form.value.subscriptionType!,
      webhookUrl: this.form.value.webhookUrl!,
      source: this.form.value.source!
    }).subscribe({
      next: (v) => {
        if (!v) this.state.set('error');
        else this.ref.close(v);
      },
      error: (err) => {
        this.state.set('error');
        console.error("Failed to create subscription", err);
      }
    })
  }
}
