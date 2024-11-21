import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';

export type MoveDirection = 'up' | 'down';

@Component({
  selector: 'hookio-accordion-header',
  standalone: true,
  imports: [
    ButtonModule
  ],
  templateUrl: './accordion-header.component.html',
  styleUrl: './accordion-header.component.scss'
})
export class AccordionHeaderComponent {
  label = input.required<string>();
  
  showMoveUp = input(false);
  showMoveDown = input(false);
  
  showCopyIcon = input(true);
  showRemoveIcon = input(true);

  move = output<MoveDirection>();
  copy = output();
  remove = output();

  emitMove(ev: Event, direction: MoveDirection) {
    ev.stopPropagation();
    this.move.emit(direction);
  }
  
  emitCopy(ev: Event) {
    ev.stopPropagation();
    this.copy.emit();
  }

  emitRemove(ev: Event) {
    ev.stopPropagation();
    this.remove.emit();
  }
}
