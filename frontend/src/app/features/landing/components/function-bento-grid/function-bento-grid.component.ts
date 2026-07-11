import {
  ChangeDetectionStrategy,
  Component,
  ViewEncapsulation,
} from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  NgxBentoGridComponent,
  NgxBentoItemComponent,
} from "@omnedia/ngx-bento-grid";
import { NgxDotpatternComponent } from "@omnedia/ngx-dotpattern";

@Component({
  selector: "app-function-bento-grid",
  imports: [
    NgxBentoGridComponent,
    NgxBentoItemComponent,
    NgxDotpatternComponent,
    RouterLink,
  ],
  template: `
    <om-bento-grid
      class="function-bento-grid"
      [columns]="2"
      itemBg="var(--color-surface)"
      itemBorder="0"
      shadowColor="transparent"
    >
      <om-bento-item [colSpan]="2">
        <ng-template #bentoBg>
          <om-dotpattern
            patternColor="rgba(71, 85, 105, 0.22)"
            styleClass="dot-pattern"
          />
        </ng-template>

        <ng-template #bentoFg>
          <article>
            <p>Coming soon</p>
            <h3 class="title">Upload</h3>
            <p class="info">Bring in your IBKR statement.</p>
          </article>
        </ng-template>
      </om-bento-item>

      <om-bento-item [colSpan]="1">
        <ng-template #bentoBg>
          <om-dotpattern
            patternColor="rgba(71, 85, 105, 0.22)"
            styleClass="dot-pattern"
          />
        </ng-template>

        <ng-template #bentoFg>
          <article>
            <p>Coming soon</p>
            <h3 class="title">Tutorial</h3>
            <p class="info">A guided tour from statement to submission.</p>
          </article>
        </ng-template>
      </om-bento-item>

      <om-bento-item [colSpan]="1">
        <ng-template #bentoBg>
          <om-dotpattern
            patternColor="rgba(71, 85, 105, 0.22)"
            styleClass="dot-pattern"
          />
        </ng-template>

        <ng-template #bentoFg>
          <a class="archive-link" routerLink="/archive">
            <h3 class="title">Archive</h3>
            <p class="info">Keep past conversions close at hand.</p>
          </a>
        </ng-template>
      </om-bento-item>
    </om-bento-grid>
  `,
  styleUrl: "./function-bento-grid.component.css",
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FunctionBentoGridComponent {}
