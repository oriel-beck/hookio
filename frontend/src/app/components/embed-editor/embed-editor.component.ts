import { ChangeDetectionStrategy, Component, computed, input, output, signal, ViewEncapsulation } from '@angular/core';
import { FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { Tab } from '../../modules/editor/editor.component';
import { ColorPicker, ColorPickerModule } from 'primeng/colorpicker';
import { EmbedForm, getEmbedFieldForm, MessageForm } from '../../modules/editor/util';
import { AccordionHeaderComponent, MoveDirection } from "./accordion-header/accordion-header.component";
import { EmbedFieldEditorComponent } from "./embed-field-editor/embed-field-editor.component";

// TODO: move all controls for the embed fields out since this component is re-rendered when there is a change above it (like embeds moving, duplicating)
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
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmbedEditorComponent {
  messageForm = input.required<MessageForm>();
  idx = input.required<number>();
  embedForm = computed(() => (this.messageForm().get('embeds') as FormArray<EmbedForm>).at(this.idx()));

  addField = output();
  moveField = output<{ fieldIdx: number, direction: MoveDirection }>();
  removeField = output<number>();
  duplicateField = output<number>();

  colorInputChanged(ev: Event, picker: ColorPicker) {
    const input = ev.target as HTMLInputElement;
    if (!input.value.startsWith("#")) input.value = "#" + input.value.slice(0, 6);
    if (input.value === "#") input.value = "";
    picker.writeValue(input.value);
  }

  // activeIndexChanged(ev: number | number[]) {
  //   console.log("field index", ev)
  //   if (Array.isArray(ev)) this.activeIndexes.set(ev);
  // }
}
