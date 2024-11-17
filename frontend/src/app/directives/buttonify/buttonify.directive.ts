import { Directive, ElementRef, HostBinding, HostListener, inject, input, OnInit, output, Renderer2, signal } from '@angular/core';

@Directive({
  selector: '[buttonify]',
  standalone: true
})
export class ButtonifyDirective implements OnInit {
  private readonly el = inject(ElementRef);
  private readonly renderer = inject(Renderer2);

  disabled = input(false);
  active = input(false);
  clicked = output<Event>();
  
  #focused = signal(false);

  ngOnInit(): void {
    this.makeButton()
  }

  private makeButton() {
    // Set role to button
    this.renderer.setAttribute(this.el.nativeElement, 'role', 'button');
  }

  @HostBinding('tabindex')
  get tabindex() {
    return this.disabled() ? -1 : 0;
  }

  @HostBinding('style.cursor')
  get cursor() {
    return this.disabled() ? 'unset' : 'pointer';
  }

  @HostBinding('attr.aria-disabled')
  @HostBinding('class.disabled')
  get isDisabled() {
    return this.disabled();
  }

  @HostBinding('class.active')
  get isActive() {
    return this.active();
  }

  @HostBinding('class.focused')
  get focused() {
    return this.#focused();
  }

  @HostListener('click', ['$event'])
  @HostListener('keyup.enter', ['$event'])
  @HostListener('keyup.space', ['$event'])
  emitClicked(ev: Event) {
    ev.preventDefault();
    ev.stopPropagation();
    if (this.disabled()) return;
    this.clicked.emit(ev);
  }

  @HostListener('focus')
  onFocus() {
    this.#focused.set(true);
  }

  @HostListener('blur')
  onBlur() {
    this.#focused.set(false);
  }
}
