import { AfterContentInit, Component, computed, inject, OnInit, signal } from '@angular/core';
import { SubscriptionService } from '../../services/subscription/subscription.service';
import { Router } from '@angular/router';
import { MessageType, Subscription, SubscriptionType } from '../../schemas/subscription.schema';
import { injectParams } from 'ngxtension/inject-params';
import { HttpErrorResponse } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { urlRegex, webhookRegex } from '../../constants';
import { TabViewModule } from 'primeng/tabview';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { toReadableFormat } from '../../util';


@Component({
  selector: 'hookio-editor',
  standalone: true,
  imports: [
    ButtonModule,
    InputTextModule,
    TabViewModule,
    ProgressSpinnerModule
  ],
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
  readonly tabs = computed(() => this.subscription()?.messages.map(m => m.type).map(v => MessageType[v] as keyof typeof MessageType).map(v => toReadableFormat(v, this.subscription()?.subscriptionType === SubscriptionType.Twitch ? "Twitch" : "YouTube")) || [])

  readonly editorOpen = signal(true);
  readonly isReady = signal(false);

  readonly subscriptionData = new FormGroup({
    webhookUrl: new FormControl<string | undefined>(undefined, [Validators.pattern(webhookRegex)]),
    source: new FormControl<string | undefined>(undefined, [Validators.required]),
    // maybe allow to change type with a warning that it resets the source
  })

  readonly embedForm = new FormGroup({
    title: new FormControl<string | undefined>(undefined),
    description: new FormControl<string | undefined>(undefined),
    url: new FormControl<string | undefined>(undefined, [Validators.pattern(urlRegex)]),
    timestamp: new FormControl<Date | undefined>(undefined),
    color: new FormControl<string | undefined>(undefined),
    footer: new FormGroup({
      text: new FormControl<string | undefined>(undefined),
      iconUrl: new FormControl<string | undefined>(undefined, [Validators.pattern(urlRegex)]),
    }),
    author: new FormGroup({
      name: new FormControl<string | undefined>(undefined),
      url: new FormControl<string | undefined>(undefined, [Validators.pattern(urlRegex)]),
      iconUrl: new FormControl<string | undefined>(undefined, [Validators.pattern(urlRegex)])
    }),
    image: new FormGroup({
      url: new FormControl<string | undefined>(undefined, [Validators.pattern(urlRegex)])
    }),
    thumbnail: new FormGroup({
      url: new FormControl<string | undefined>(undefined, [Validators.pattern(urlRegex)])
    }),
    fields: new FormArray<FormGroup<{ name: FormControl<string>, value: FormControl<string>, inline: FormControl<string> }>>([])
  });

  ngOnInit(): void {
    const subscriptionAsNumber = Number(this.subscriptionId());
    if (isNaN(subscriptionAsNumber)) this.router.navigate(["servers", this.guildId()]);
    else this.subscriptionService.getSubscriptions(this.guildId(), subscriptionAsNumber).subscribe({
      next: (v) => this.subscription.set(v),
      error: (e: HttpErrorResponse) => {
        if (e.status === 403) this.router.navigate(["servers", this.guildId()]);
      }
    });

    // avoid applying animations on entering
    setTimeout(() => {
      this.isReady.set(true);
    });
  }
}
