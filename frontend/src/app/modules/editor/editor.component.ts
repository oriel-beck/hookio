import { Component, computed, effect, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { injectParams } from 'ngxtension/inject-params';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { TabViewModule } from 'primeng/tabview';
import { MoveDirection } from "../../components/embed-editor/accordion-header/accordion-header.component";
import { LoadingOverlayComponent } from "../../components/loading-overlay/loading-overlay.component";
import { EditorStore } from '../../store/editor.store';
import { EditorDrawerComponent } from './editor-drawer/editor-drawer.component';
import { EditorPreviewComponent } from "./editor-preview/editor-preview.component";

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

@Component({
  selector: 'hookio-editor',
  standalone: true,
  imports: [
    ButtonModule,
    TabViewModule,
    DividerModule,
    ScrollPanelModule,
    LoadingOverlayComponent,
    EditorDrawerComponent,
    EditorPreviewComponent
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

  readonly editorHidden = signal(false);
  readonly isReady = signal(false);

  accordionState = signal<AccordionState>({});
  activeTabIndex = signal(0);

  currentMessage = computed(() => this.store.messages().at(this.activeTabIndex()));

  ngOnInit(): void {
    const subscriptionAsNumber = Number(this.subscriptionId());
    if (isNaN(subscriptionAsNumber)) this.router.navigate(["servers", this.guildId()]);
    else {
      this.store.init(this.guildId(), subscriptionAsNumber);
      this.store.fetchSubscription();
    }
  }

  
}
