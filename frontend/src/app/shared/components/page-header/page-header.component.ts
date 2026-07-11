import { ChangeDetectionStrategy, Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";

@Component({
  selector: "app-page-header",
  imports: [RouterLink],
  template: `
    <header class="page-header">
      <a class="page-header__link" [routerLink]="link()">{{ linkText() }}</a>
      <h1>{{ title() }}</h1>
    </header>
  `,
  styles: `
    :host {
      display: block;
      width: min(100%, 70rem);
    }

    .page-header {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
      align-items: center;
    }

    .page-header__link {
      width: fit-content;
      color: var(--color-text-muted);
      text-decoration: none;
    }

    .page-header__link:hover {
      color: var(--color-text);
      text-decoration: underline;
    }

    h1 {
      grid-column: 2;
      margin: 0;
      font-size: clamp(2rem, 6vw, 3.5rem);
      text-align: center;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
  readonly link = input.required<string>();
  readonly linkText = input.required<string>();
}
