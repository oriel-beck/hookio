import { Component, inject } from '@angular/core';
import { UserService } from '../../services/user/user.service';
import { GuildIconComponent } from '../../components/guild-icon/guild-icon.component';
import { Router } from '@angular/router';
import { User } from '../../schemas/user.schema';

@Component({
  selector: 'hookio-guilds',
  standalone: true,
  imports: [GuildIconComponent],
  templateUrl: './guilds.component.html',
  styleUrl: './guilds.component.scss'
})
export class GuildsComponent {
  private readonly userService = inject(UserService);
  private readonly router = inject(Router)

  user = this.userService.user;

  navigateToGuildDashboard(guildId: User['guilds'][0]['id']) {
    this.router.navigate(["servers", guildId]);
  }
}
