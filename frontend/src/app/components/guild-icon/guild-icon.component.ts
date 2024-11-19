import { Component, computed, input } from '@angular/core';
import { AvatarModule } from 'primeng/avatar';
import { User } from '../../schemas/user.schema';
import { ButtonifyDirective } from '../../directives/buttonify/buttonify.directive';
import { defaultDiscordIcon } from '../../constants';

@Component({
  selector: 'hookio-guild-icon',
  standalone: true,
  imports: [AvatarModule],
  templateUrl: './guild-icon.component.html',
  styleUrl: './guild-icon.component.scss',
  hostDirectives: [{
    directive: ButtonifyDirective,
    outputs: ['clicked']
  }]
})
export class GuildIconComponent {
  guild = input.required<User['guilds'][0]>();

  guildIcon = computed(() => this.guild().iconUrl || defaultDiscordIcon)
}
