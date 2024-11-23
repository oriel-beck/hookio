import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { AccordionModule } from 'primeng/accordion';
import { CheckboxModule } from 'primeng/checkbox';
import { FloatLabelModule } from 'primeng/floatlabel';
import { AccordionHeaderComponent, MoveDirection } from '../accordion-header/accordion-header.component';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import { EmbedFieldForm, EmbedForm } from '../../../modules/editor/util';

@Component({
  selector: 'hookio-embed-field-editor',
  standalone: true,
  imports: [
    FloatLabelModule,
    CheckboxModule,
    AccordionModule,
    AccordionHeaderComponent,
    ReactiveFormsModule
  ],
  templateUrl: './embed-field-editor.component.html',
  styleUrl: './embed-field-editor.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmbedFieldEditorComponent {
  embedForm = input.required<EmbedForm>();
  embedIdx = input.required<number>();

  moveField = output<{ fieldIdx: number, direction: MoveDirection }>();
  duplicateField = output<number>();
  removeField = output<number>();
}
