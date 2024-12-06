import { Component, HostBinding, input, model, output } from '@angular/core';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { TabViewModule } from 'primeng/tabview';
import { AccordionHeaderComponent, MoveDirection } from '../../../components/embed-editor/accordion-header/accordion-header.component';
import { ToMessageLabelPipe } from '../../../pipes/to-message-label/to-message-label.pipe';
import type { EditorState } from '../../../store/editor.store';
import type { MessageForm } from '../util';
import { EmbedEditorComponent } from '../../../components/embed-editor/embed-editor.component';
import { AccordionState } from '../editor.component';

@Component({
  selector: 'hookio-editor-drawer',
  standalone: true,
  imports: [
    ButtonModule,
    ReactiveFormsModule,
    AccordionModule,
    AccordionHeaderComponent,
    TabViewModule,
    ScrollPanelModule,
    ToMessageLabelPipe,
    FloatLabelModule,
    InputTextModule,
    InputTextareaModule,
    DividerModule,
    EmbedEditorComponent
  ],
  templateUrl: './editor-drawer.component.html',
  styleUrl: './editor-drawer.component.scss',
  host: {
    class: 'editor-drawer'
  }
})
export class EditorDrawerComponent {
  hidden = model<boolean>(false);
  accordionState = model.required<AccordionState>();
  activeTabIndex = model<number>(0);

  ready = input.required<boolean>();
  form = input.required<EditorState['form']>();
  subscription = input.required<EditorState['subscription']>();
  messages = input.required<FormArray<MessageForm>>();

  addEmbedOutput = output<{ messageIdx: number }>({ alias: 'addEmbed' });
  moveEmbedOutput = output<{ messageIdx: number, embedIdx: number, direction: MoveDirection }>({ alias: 'moveEmbed' });
  duplicateEmbedOutput = output<{ messageIdx: number, embedIdx: number }>({ alias: 'duplicateEmbed' });
  removeEmbedOutput = output<{ messageIdx: number, embedIdx: number }>({ alias: 'removeEmbed' });

  addEmbedFieldOutput = output<{ messageIdx: number, embedIdx: number }>({ alias: 'addEmbedField' });
  moveEmbedFieldOutput = output<{ messageIdx: number, embedIdx: number, fieldIdx: number, direction: MoveDirection }>({ alias: 'moveEmbedField' });
  duplicateEmbedFieldOutput = output<{ messageIdx: number, embedIdx: number, fieldIdx: number }>({ alias: 'duplicateEmbedField' });
  removeEmbedFieldOutput = output<{ messageIdx: number, embedIdx: number, fieldIdx: number }>({ alias: 'removeEmbedField' });

  @HostBinding('class.hidden') get isHidden() {
    return this.hidden();
  }

  @HostBinding('class.ready') get isReady() {
    return this.ready();
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

    this.addEmbedOutput.emit({ messageIdx })
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

    this.moveEmbedOutput.emit({ messageIdx, embedIdx, direction });
  }

  duplicateEmbed(messageIdx: number, embedIdx: number) {
    // Max embeds is 10
    if ((this.form()?.value.messages?.at(messageIdx)?.embeds?.length || 0) >= 10) return;

    this.accordionState.update((data) => {
      const tmp = { ...data };
      const embed = tmp[messageIdx];
      // Shift all numbers higher than the current embed up by 1 since I'm pushing a new embed above them
      embed!.activeIndexes = embed!.activeIndexes.map((v) => v > embedIdx ? v + 1 : v);
      return tmp;
    });

    this.duplicateEmbedOutput.emit({ messageIdx, embedIdx });
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

    this.removeEmbedOutput.emit({ messageIdx, embedIdx })
  }
}
