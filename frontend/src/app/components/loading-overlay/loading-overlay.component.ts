import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'hookio-loading-overlay',
  standalone: true,
  imports: [
    ProgressSpinnerModule
  ],
  templateUrl: './loading-overlay.component.html',
  styleUrl: './loading-overlay.component.scss',
  host: {
    'aria-hidden': 'true'
  },
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingOverlayComponent {

}
