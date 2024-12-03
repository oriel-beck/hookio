import { Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { injectParams } from 'ngxtension/inject-params';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { TabViewModule } from 'primeng/tabview';
import { AccordionHeaderComponent, MoveDirection } from "../../components/embed-editor/accordion-header/accordion-header.component";
import { EmbedEditorComponent } from "../../components/embed-editor/embed-editor.component";
import { LoadingOverlayComponent } from "../../components/loading-overlay/loading-overlay.component";
import { ToMessageLabelPipe } from "../../pipes/to-message-label/to-message-label.pipe";
import { Message } from '../../schemas/subscription.schema';
import { EditorStore } from '../../store/editor.store';
import { getEmbedForm, getMessageForm } from './util';

export type AccordionState = {
  [messageIdx: number]: {
    activeIndexes: number[];
    embeds: {
      [embedIdx: number]: {
        activeIndexes: number[]; // Tracks open embed tabs
        fields: number[] // tracks embedFields
      };
    } | undefined;
  } | undefined;
}

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
    AccordionHeaderComponent,
    ToMessageLabelPipe
  ],
  providers: [EditorStore],
  templateUrl: './editor.component.html',
  styleUrl: './editor.component.scss'
})
export class EditorComponent implements OnInit {
  constructor() {
    effect(() => {
      const subscription = this.store.subscription();
      if (subscription) {
        // Set all existing messages
        const state: AccordionState = subscription.messages.reduce((acc, msg, i) => ({
          ...acc, [i]: {
            activeIndexes: [],
            // Make sure to set all existing embeds
            embeds: msg.embeds.reduce((acc, _, i) => ({
              ...acc,
              [i]: {
                activeIndexes: [],
                fields: []
              }
            }), {} as {
              [embedIdx: number]: {
                activeIndexes: number[];
                fields: number[];
              };
            })
          }
        }), {} as AccordionState);
        this.accordionState.set(state);
      }
    }, { allowSignalWrites: true });

    effect(() => {
      const loading = this.store.loading();
      if (!loading) {
        // avoid applying animations on entering
        setTimeout(() => {
          this.isReady.set(true);
        });
      }
    });
  }

  readonly store = inject(EditorStore);
  private readonly router = inject(Router);
  private params = injectParams();

  readonly guildId = computed<string>(() => this.params()['guildId']);
  readonly subscriptionId = computed<string>(() => this.params()['subscriptionId']);

  readonly editorOpen = signal(true);
  readonly isReady = signal(false);

  accordionState = signal<AccordionState>({});

  ngOnInit(): void {
    const subscriptionAsNumber = Number(this.subscriptionId());
    if (isNaN(subscriptionAsNumber)) this.router.navigate(["servers", this.guildId()]);
    else {
      this.store.init(this.guildId(), subscriptionAsNumber);
      this.store.fetchSubscription();
    }
  }

  accordionStateChanged(messageIdx: number, fields: number | number[]) {
    if (!Array.isArray(fields)) return;
    this.accordionState.update((data) => {
      const tmp = { ...data };
      tmp[messageIdx]!.activeIndexes = fields;
      return tmp;
    })
  }

  addEmbed(messageIdx: number) {
    const keys = Object.keys(this.accordionState()![messageIdx]!.embeds!);
    // Max embeds is 10
    if (keys.length >= 10) return;
    this.accordionState.update((data) => {
      const tmp = { ...data };
      // Add the new embed index to the state
      tmp[messageIdx]!.embeds![keys.length] = { activeIndexes: [], fields: [] };
      return tmp;
    });

    this.store.addEmbed(messageIdx)
  }

  moveEmbed(messageIdx: number, embedIdx: number, direction: MoveDirection) {
    this.accordionState.update((data) => {
      const tmp = { ...data };
      const message = tmp[messageIdx]!;
      const target = direction === 'up' ? embedIdx - 1 : embedIdx + 1;
      const targetIdx = message.activeIndexes.indexOf(target);
      const currentIdx = message.activeIndexes.indexOf(embedIdx);
      // If both exists, do nothing, they are both open
      if (targetIdx > -1 && currentIdx > -1) return tmp;
      // If only target exists, replace with current index to keep it open
      if (targetIdx > -1) message.activeIndexes.splice(targetIdx, 1, embedIdx);
      // If only current exists, replace with target index to keep it open
      if (currentIdx > -1) message.activeIndexes.splice(currentIdx, 1, target);
      return tmp;
    });

    this.store.moveEmbed(messageIdx, embedIdx, direction);
  }

  duplicateEmbed(messageIdx: number, embedIdx: number) {
    // Max embeds is 10
    if ((this.store.form()?.value.messages?.at(messageIdx)?.embeds?.length || 0) >= 10) return;

    this.accordionState.update((data) => {
      const tmp = { ...data };
      const embed = tmp[messageIdx];
      // Shift all numbers higher than the current embed up by 1 since I'm pushing a new embed above them
      embed!.activeIndexes = embed!.activeIndexes.map((v) => v > embedIdx ? v + 1 : v);
      return tmp;
    });

    this.store.duplicateEmbed(messageIdx, embedIdx);
  }

  removeEmbed(messageIdx: number, embedIdx: number) {
    this.accordionState.update((data) => {
      const tmp = { ...data };
      const embed = tmp[messageIdx];
      const idx = embed!.activeIndexes.indexOf(embedIdx);
      // If current embed is open, remove its index 
      if (idx > -1) embed!.activeIndexes.splice(idx, 1);
      // Shift all numbers higher than the removed embed down by 1 since I'm removing the embed above them
      embed!.activeIndexes = embed!.activeIndexes.map((v) => v > embedIdx ? v - 1 : v);
      return tmp;
    });

    this.store.removeEmbed(messageIdx, embedIdx)
  }
}
