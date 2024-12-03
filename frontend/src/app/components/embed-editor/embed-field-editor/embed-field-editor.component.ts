import { Component, input, model, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { CheckboxModule } from 'primeng/checkbox';
import { FloatLabelModule } from 'primeng/floatlabel';
import { AccordionState } from '../../../modules/editor/editor.component';
import { EmbedForm } from '../../../modules/editor/util';
import { AccordionHeaderComponent, MoveDirection } from '../accordion-header/accordion-header.component';
import { InputTextareaModule } from 'primeng/inputtextarea';

@Component({
  selector: 'hookio-embed-field-editor',
  standalone: true,
  imports: [
    FloatLabelModule,
    InputTextareaModule,
    CheckboxModule,
    AccordionModule,
    AccordionHeaderComponent,
    ReactiveFormsModule
  ],
  templateUrl: './embed-field-editor.component.html',
  styleUrl: './embed-field-editor.component.scss',
})
export class EmbedFieldEditorComponent {
  embedForm = input.required<EmbedForm>();
  embedIdx = input.required<number>();
  messageIdx = input.required<number>();

  moveFieldOutput = output<{ fieldIdx: number, direction: MoveDirection }>({ alias: 'moveField' });
  duplicateFieldOutput = output<number>({ alias: 'duplicateField' });
  removeFieldOutput = output<number>({ alias: 'removeField' });

  accordionState = model.required<AccordionState>();

  accordionStateChanged(fields: number | number[]) {
    if (!Array.isArray(fields)) return;
    this.accordionState.update((data) => {
      const tmp = { ...data };
      tmp[this.messageIdx()]!.embeds![this.embedIdx()].fields = fields;
      return tmp;
    });
  }

  moveField(fieldIdx: number, direction: MoveDirection) {
    this.accordionState.update((data) => {
      const tmp = { ...data };
      const embed = tmp[this.messageIdx()]!.embeds![this.embedIdx()];
      const target = direction === 'up' ? fieldIdx - 1 : fieldIdx + 1;
      const targetIdx = embed.fields.indexOf(target);
      const currentIdx = embed.fields.indexOf(fieldIdx);
      // If both exists, do nothing, they are both open
      if (targetIdx > -1 && currentIdx > -1) return tmp;
      // If only target exists, replace with current index to keep it open
      if (targetIdx > -1) embed.fields.splice(targetIdx, 1, fieldIdx);
      // If only current exists, replace with target index to keep it open
      if (currentIdx > -1) embed.fields.splice(currentIdx, 1, target);
      return tmp;
    });
    this.moveFieldOutput.emit({ fieldIdx, direction });
  }

  duplicateField(fieldIdx: number) {
    // Max fields is 25
    if ((this.embedForm().value.fields?.length || 0) >= 25) return;

    this.accordionState.update((data) => {
      const tmp = { ...data };
      const embed = tmp[this.messageIdx()]!.embeds![this.embedIdx()];
      // Shift all numbers higher than the current field up by 1 since I'm pushing a new field above them
      embed.fields = embed.fields.map((v) => v > fieldIdx ? v + 1 : v);
      return tmp;
    });
    this.duplicateFieldOutput.emit(fieldIdx)
  }

  removeField(fieldIdx: number) {
    this.accordionState.update((data) => {
      const tmp = { ...data };
      const embed = tmp[this.messageIdx()]!.embeds![this.embedIdx()];
      const idx = embed.fields.indexOf(fieldIdx);
      if (idx > -1) embed.fields.splice(idx, 1);
      // Shift all numbers higher than the removed field down by 1 since I'm removing a field above them
      embed.fields = embed.fields.map((v) => v > fieldIdx ? v - 1 : v);
      return tmp;
    });
    this.removeFieldOutput.emit(fieldIdx);
  }
}
