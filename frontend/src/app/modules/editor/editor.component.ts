import { Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { SubscriptionService } from '../../services/subscription/subscription.service';
import { Router } from '@angular/router';
import { Embed, Message, MessageType, Subscription, SubscriptionType } from '../../schemas/subscription.schema';
import { injectParams } from 'ngxtension/inject-params';
import { HttpErrorResponse } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TabViewModule } from 'primeng/tabview';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { toReadableFormat } from '../../util';
import { AccordionModule } from 'primeng/accordion';
import { getEmbedFieldForm, getEmbedForm, getMessageForm, getSubscriptionForm } from './util';
import { DividerModule } from 'primeng/divider';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ColorPicker, ColorPickerModule } from 'primeng/colorpicker';
import { LoadingOverlayComponent } from "../../components/loading-overlay/loading-overlay.component";
import { EmbedEditorComponent } from "../../components/embed-editor/embed-editor.component";
import { AccordionHeaderComponent, MoveDirection } from "../../components/embed-editor/accordion-header/accordion-header.component";

export interface Tab {
  label: string;
  value: Message;
  messageForm: ReturnType<typeof getMessageForm>;
  embedsForm: FormArray<ReturnType<typeof getEmbedForm>>;
}

@Component({
  selector: 'hookio-editor',
  standalone: true,
  imports: [
    ButtonModule,
    TabViewModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    AccordionModule,
    DividerModule,
    InputTextModule,
    InputTextareaModule,
    FloatLabelModule,
    ScrollPanelModule,
    LoadingOverlayComponent,
    EmbedEditorComponent,
    AccordionHeaderComponent
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
  readonly subscriptionForm = computed(() => getSubscriptionForm(this.subscription()));
  readonly tabs = computed<Tab[]>(() => {
    const forms = this.subscriptionForm().get('messages') as FormArray<ReturnType<typeof getMessageForm>>;
    const messages = this.subscription()?.messages || [];
    return messages.map((message, i) => ({
      label: toReadableFormat(MessageType[message.type] as keyof typeof MessageType, this.subscription()?.subscriptionType === SubscriptionType.Twitch ? "Twitch" : "YouTube"),
      value: message,
      messageForm: forms.at(i),
      embedsForm: forms.at(i).get('embeds') as FormArray<ReturnType<typeof getEmbedForm>>
    }));
  });
  // only load after tabs are built
  readonly loading = computed(() => !this.subscription() && this.tabs().length);

  readonly editorOpen = signal(true);
  readonly isReady = signal(false);

  readonly activeIndexes = signal<number[]>([]);

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

  addEmbedTo(embedsForm: ReturnType<typeof this.tabs>[0]['embedsForm']) {
    if (embedsForm.value.length === 10) return;
    embedsForm.push(getEmbedForm());
  }

  duplicateEmbed(idx: number, embedsForm: ReturnType<typeof this.tabs>[0]['embedsForm']) {
    const origin = embedsForm.at(idx);
    if (origin && embedsForm.value.length !== 10) embedsForm.insert(idx, getEmbedForm(origin.value as Embed));
    this.activeIndexes.update(i => {
      // shift all indexes above the current index by 1
      const mapped = i.map(cidx => cidx > idx ? cidx + 1 : cidx);
      // add the new index in
      mapped.splice(idx, 0, idx + 1);
      return mapped;
    })
  }

  removeEmbed(idx: number, embedsForm: ReturnType<typeof this.tabs>[0]['embedsForm']) {
    embedsForm.removeAt(idx);
    this.activeIndexes.update(i => {
      // remove the removed index
      i.splice(idx, 1);
      return [...i];
    });
  }

  swapEmbed(origin: number, direction: MoveDirection, embedsForm: ReturnType<typeof this.tabs>[0]['embedsForm']) {
    const target = direction === 'down' ? origin + 1 : origin - 1;
    const originControl = getEmbedForm(embedsForm.at(origin).value as Embed);
    const targetControl = getEmbedForm(embedsForm.at(target).value as Embed);
    embedsForm.setControl(origin, targetControl);
    embedsForm.setControl(target, originControl);
  }

  activeIndexChanged(ev: number | number[]) {
    console.log("index", ev)
    if (Array.isArray(ev)) this.activeIndexes.set(ev);
  }
}
