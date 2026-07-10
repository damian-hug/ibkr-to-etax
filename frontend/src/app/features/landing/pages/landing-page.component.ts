import { ChangeDetectionStrategy, Component } from "@angular/core";

import { FunctionBentoGridComponent } from "../components/function-bento-grid/function-bento-grid.component";

@Component({
  selector: "app-landing-page",
  imports: [FunctionBentoGridComponent],
  template: `
    <main class="landing">
      <header class="landing__header">
        <h1>Statement for E-Tax from IBKR-Report</h1>
        <p>
          Turn your IBKR activity statement into a clear, organized eTax
          workflow to simplify your tax filing process.
        </p>
      </header>

      <section class="landing__functions" aria-labelledby="functions-title">
        <app-function-bento-grid />
      </section>
    </main>
  `,
  styles: `
    .landing {
      display: flex;
      min-height: 100dvh;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: 1rem;
    }

    .landing__header,
    .landing__functions {
      width: min(100% - 2rem, 70rem);
    }

    .landing__functions {
      margin-block: auto;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingPageComponent {}
