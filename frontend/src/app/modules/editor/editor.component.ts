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
import { EditorStore } from '../../store/editor.store';
import { ToMessageLabelPipe } from "../../pipes/to-message-label/to-message-label.pipe";

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
  // new code
  readonly store = inject(EditorStore);
  private readonly router = inject(Router);
  private params = injectParams();

  readonly guildId = computed<string>(() => this.params()['guildId']);
  readonly subscriptionId = computed<string>(() => this.params()['subscriptionId']);

  readonly editorOpen = signal(true);
  readonly isReady = signal(false);

  ngOnInit(): void {
    const subscriptionAsNumber = Number(this.subscriptionId());
    if (isNaN(subscriptionAsNumber)) this.router.navigate(["servers", this.guildId()]);
    else {
      this.store.init(this.guildId(), subscriptionAsNumber);
      this.store.fetchSubscription();
    }

    // TODO: move this to only apply after the loading is set to false, in a setTimeout to be out of the event cycle
    // avoid applying animations on entering
    setTimeout(() => {
      this.isReady.set(true);
    });
  }
}
