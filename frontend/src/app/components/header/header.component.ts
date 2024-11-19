import { Component, computed, inject, input, output } from '@angular/core';
import { User } from '../../schemas/user.schema';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { ButtonifyDirective } from '../../directives/buttonify/buttonify.directive';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { Router } from '@angular/router';
import { defaultDiscordIcon } from '../../constants';

@Component({
  selector: 'hookio-header',
  standalone: true,
  imports: [
    ToolbarModule,
    ButtonModule,
    AvatarModule,
    ButtonifyDirective,
    MenuModule
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  private readonly router = inject(Router);

  items: MenuItem[] = [
    {
      label: "Servers",
      command: () => this.router.navigate(["servers"])
    },
    {
      label: "Log out",
      command: () => location.replace("/api/users/logout")
    }
  ]

  user = input<User>();
  userAvatar = computed(() => this.user()?.user.avatarUrl || defaultDiscordIcon)

  login() {
    location.replace("/api/users/login");
  }
}
