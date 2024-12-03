import { Component, computed, input, model, output } from '@angular/core';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { ColorPicker, ColorPickerModule } from 'primeng/colorpicker';
import { DividerModule } from 'primeng/divider';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { AccordionState } from '../../modules/editor/editor.component';
import { EmbedForm, MessageForm } from '../../modules/editor/util';
import { MoveDirection } from "./accordion-header/accordion-header.component";
import { EmbedFieldEditorComponent } from "./embed-field-editor/embed-field-editor.component";

@Component({
  selector: 'hookio-embed-editor',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AccordionModule,
    DividerModule,
    InputTextModule,
    InputTextareaModule,
    FloatLabelModule,
    ButtonModule,
    ColorPickerModule,
    EmbedFieldEditorComponent
  ],
  templateUrl: './embed-editor.component.html',
  styleUrl: './embed-editor.component.scss',
})
export class EmbedEditorComponent {
  messageForm = input.required<MessageForm>();
  embedForm = computed(() => (this.messageForm().get('embeds') as FormArray<EmbedForm>).at(this.idx()));

  messageIdx = input.required<number>();
  idx = input.required<number>();

  addField = output();
  moveField = output<{ fieldIdx: number, direction: MoveDirection }>();
  removeField = output<number>();
  duplicateField = output<number>();

  accordionState = model.required<AccordionState>();

  accordionStateChanged(fields: number | number[]) {
    console.log(this.accordionState(), this.idx(), this.messageIdx())
    if (!Array.isArray(fields)) return;
    this.accordionState.update((data) => {
      const tmp = { ...data };
      tmp[this.messageIdx()]!.embeds![this.idx()].activeIndexes = fields;
      return tmp;
    });
  }

  colorInputChanged(ev: Event, picker: ColorPicker) {
    const input = ev.target as HTMLInputElement;
    if (!input.value.startsWith("#")) input.value = "#" + input.value.slice(0, 6);
    if (input.value === "#") input.value = "";
    picker.writeValue(input.value);
  }
}
