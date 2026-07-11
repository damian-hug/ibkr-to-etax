import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { NgxDotpatternComponent } from "@omnedia/ngx-dotpattern";
import { ButtonModule } from "primeng/button";

import { PageHeaderComponent } from "../../../shared/components/page-header/page-header.component";
import { ArchiveListComponent } from "../components/archive-list.component";
import { ArchiveStore } from "../store/archive.store";

@Component({
  selector: "app-archive-page",
  imports: [
    ArchiveListComponent,
    ButtonModule,
    NgxDotpatternComponent,
    PageHeaderComponent,
  ],
  template: `
    <main class="archive">
      <app-page-header
        title="Archive"
        link="/"
        linkText="← Back to overview"
      />

      <section class="archive__content">
        <om-dotpattern
          patternColor="rgba(71, 85, 105, 0.22)"
          styleClass="dot-pattern"
        >
          <div class="archive__body">
            @if (store.error()) {
              <div class="archive__state archive__state--error" role="alert">
                <div>
                  <h2>The archive could not be loaded</h2>
                  <p>Check that the server is running, then try again.</p>
                </div>
                <button pButton type="button" (click)="store.reload()">
                  Try again
                </button>
              </div>
            } @else if (store.isLoading()) {
              <div class="archive__state" aria-live="polite">
                <p>Loading archive…</p>
              </div>
            } @else if (store.value().length === 0) {
              <div class="archive__state">
                <h2>No generated statements yet</h2>
              </div>
            } @else {
              <app-archive-list [items]="store.value()" />
            }
          </div>
        </om-dotpattern>
      </section>
    </main>
  `,
  styles: `
    :host {
      display: block;
      min-height: 100dvh;
      background: var(--color-background);
    }

    .archive {
      display: flex;
      min-height: 100dvh;
      width: 100%;
      flex-direction: column;
      align-items: center;
    }

    h2,
    p {
      margin-top: 0;
    }

    .archive__content {
      overflow: hidden;
      width: min(100%, 70rem);
      margin-top: 2rem;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      background: var(--color-surface);
    }

    om-dotpattern {
      display: block;
    }

    .archive__body {
      position: relative;
      z-index: 1;
      padding: 0;
    }

    .archive__state {
      display: flex;
      flex: 1;
      min-height: 12rem;
      align-items: center;
      justify-content: center;
      flex-direction: column;
      text-align: center;
    }

    .archive__state--error {
      gap: 1rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArchivePageComponent {
  protected readonly store = inject(ArchiveStore);
}
