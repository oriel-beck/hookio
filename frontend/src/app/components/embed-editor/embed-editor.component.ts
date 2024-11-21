import { Component, computed, input, output, signal, ViewEncapsulation } from '@angular/core';
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
import { getEmbedFieldForm } from '../../modules/editor/util';
import { AccordionHeaderComponent, MoveDirection } from "./accordion-header/accordion-header.component";

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
    CheckboxModule,
    AccordionHeaderComponent
  ],
  templateUrl: './embed-editor.component.html',
  styleUrl: './embed-editor.component.scss'
})
export class EmbedEditorComponent {
  tab = input.required<Tab>();
  idx = input.required<number>();
  embedForm = computed(() => this.tab().embedsForm.at(this.idx()));
  swap = output<{ origin: number, target: number }>();
  remove = output<number>();
  duplicate = output<number>();

  activeIndexes = signal<number[]>([]);

  colorInputChanged(ev: Event, picker: ColorPicker) {
    const input = ev.target as HTMLInputElement;
    if (!input.value.startsWith("#")) input.value = "#" + input.value.slice(0, 6);
    if (input.value === "#") input.value = "";
    picker.writeValue(input.value);
  }

  addField() {
    const fieldsControl = this.embedForm().get('fields') as FormArray<ReturnType<typeof getEmbedFieldForm>>;
    if (fieldsControl.value.length === 25) return;
    fieldsControl.push(getEmbedFieldForm());
    this.activeIndexes.update(idx => idx.map(i => i >= 3 ? i + 1 : i));
  }

  removeField(idx: number) {
    const fieldsControl = this.embedForm().get('fields') as FormArray<ReturnType<typeof getEmbedFieldForm>>;
    fieldsControl.removeAt(idx);
  }

  swapField(origin: number, direction: MoveDirection) {
    const target = origin + direction === 'down' ? 1 : -1;
    const fieldsControl = this.embedForm().get('fields') as FormArray<ReturnType<typeof getEmbedFieldForm>>;
    const originField = fieldsControl.at(origin)
    const targetField = fieldsControl.at(target);
    fieldsControl.setControl(target, originField);
    fieldsControl.setControl(origin, targetField);
  }

  duplicateField(idx: number) {
    const fieldsControl = this.embedForm().get('fields') as FormArray<ReturnType<typeof getEmbedFieldForm>>;
    fieldsControl.insert(idx, fieldsControl.at(idx));
  }

  activeIndexChanged(ev: number | number[]) {
    if (Array.isArray(ev)) this.activeIndexes.set(ev);
  }

  moveEmbed(direction: MoveDirection) {
    this.swap.emit({ origin: this.idx(), target: this.idx() + direction === 'down' ? 1 : -1 })
  }
}
