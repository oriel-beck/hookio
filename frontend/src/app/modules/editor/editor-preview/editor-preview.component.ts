import { Component, CUSTOM_ELEMENTS_SCHEMA, HostBinding, inject, input, SecurityContext } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { toHTML } from '@odiffey/discord-markdown';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { Subscription } from '../../../schemas/subscription.schema';
import { MessageForm } from '../util';

@Component({
  selector: 'hookio-editor-preview',
  standalone: true,
  imports: [
    ScrollPanelModule,
  ],
  templateUrl: './editor-preview.component.html',
  styleUrl: './editor-preview.component.scss',
  host: {
    class: 'editor-preview'
  },
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class EditorPreviewComponent {
  private readonly sanitizer = inject(DomSanitizer);
  full = input.required<boolean>();
  subscription = input.required<Subscription>();
  message = input.required<MessageForm>();

  @HostBinding('class.full') get isFull() {
    return this.full();
  }

  sanitize(text?: string | null) {
    if (!text) return "";
    return this.sanitizer.sanitize(SecurityContext.NONE, text)!;
  }

  format(text?: string | null) {
    if (!text) return "";
    const sanitized = this.sanitize(text);
    const parsed = toHTML(sanitized, {
      discordOnly: true, discordCallback: {
        user: () => `<discord-mention type="user">user</discord-mention>`,
        role: () => `<discord-mention type="role">role</discord-mention>`,
        channel: () => `<discord-mention type="channel">channel</discord-mention>`
      }
    }).replaceAll('\n', '<br/>');
    return this.sanitizer.bypassSecurityTrustHtml(parsed)
  }

  computeInlineIndex(fields: { inline?: boolean | null }[], currentIndex: number): number {
    if (!fields[currentIndex]?.inline) return 0; // Not inline, no index

    let inlineIndex = 1; // Start at index 1
    for (let i = currentIndex - 1; i >= 0; i--) {
      if (!fields[i].inline) break; // Break if previous field is not inline
      inlineIndex++; // Increment index for each prior inline field
      if (inlineIndex > 3) inlineIndex = 1; // Wrap around at 3
    }

    return inlineIndex;
  }
}
