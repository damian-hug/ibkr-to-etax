import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-root',
  imports: [ButtonModule],
  template: '<p-button label="hello ibkr-to-etax" />',
  styles: [
    `
      :host {
        display: grid;
        min-height: 100dvh;
        place-items: center;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
