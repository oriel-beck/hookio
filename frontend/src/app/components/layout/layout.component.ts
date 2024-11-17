import { Component, input, output } from '@angular/core';
import { HeaderComponent } from "../header/header.component";
import { User } from '../../schemas/user.schema';

@Component({
  selector: 'hookio-layout',
  standalone: true,
  imports: [HeaderComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  user = input<User>();
}
