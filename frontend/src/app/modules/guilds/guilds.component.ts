import { Component, inject } from '@angular/core';
import { UserService } from '../../services/user/user.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'hookio-guilds',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './guilds.component.html',
  styleUrl: './guilds.component.scss'
})
export class GuildsComponent {
  private readonly userService = inject(UserService);

  user = this.userService.user;
}
