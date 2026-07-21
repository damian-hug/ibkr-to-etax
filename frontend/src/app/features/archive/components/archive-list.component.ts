import { DatePipe } from "@angular/common";
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
} from "@angular/core";
import { MenuItem, MessageService } from "primeng/api";
import { SplitButtonModule } from "primeng/splitbutton";
import { TableModule } from "primeng/table";
import { ToastModule } from "primeng/toast";

import {
  ArchiveFileDto,
  ArchiveFileType,
  ArchiveItemDto,
} from "../../../core/services/etax-api.models";

@Component({
  selector: "app-archive-list",
  imports: [DatePipe, SplitButtonModule, TableModule, ToastModule],
  providers: [MessageService],
  template: `
    <p-toast position="top-right" />

    <p-table
      [value]="items()"
      [tableStyle]="{ 'min-width': '46rem' }"
      showGridlines
      stripedRows
    >
      <ng-template #header>
        <tr>
          <th>Name</th>
          <th class="archive-table__date-column">Date</th>
          <th class="archive-table__download-column">Download</th>
        </tr>
      </ng-template>

      <ng-template #body let-item>
        <tr
          class="archive-table__row"
          [class.archive-table__row--previewable]="pdfFile(item)"
          [attr.tabindex]="pdfFile(item) ? 0 : null"
          [attr.aria-label]="
            pdfFile(item) ? 'Preview PDF for ' + item.name : null
          "
          (click)="previewPdf(item)"
          (keydown.enter)="previewPdf(item)"
          (keydown.space)="previewPdf(item, $event)"
        >
          <td class="archive-table__name">
            <span>{{ item.name }}</span>
            @if (pdfFile(item)) {
              <span
                class="pi pi-eye archive-table__preview-icon"
                aria-hidden="true"
              ></span>
            }
          </td>
          <td class="archive-table__date-column">
            <time [attr.datetime]="item.generatedAt">
              {{ item.generatedAt | date: "dd.MM.yyyy" }}
            </time>
          </td>
          <td class="archive-table__download-column">
            <div
              class="archive-table__download"
              (click)="$event.stopPropagation()"
            >
              <p-splitbutton
                label="Download"
                [model]="downloadItemsByItem().get(item)"
                appendTo="body"
                expandAriaLabel="Choose download format"
                (onClick)="downloadPdf(item)"
              >
                <ng-template #content>
                  <span
                    class="pi pi-download archive-table__download-icon"
                    aria-hidden="true"
                  ></span>
                </ng-template>
              </p-splitbutton>
            </div>
          </td>
        </tr>
      </ng-template>
    </p-table>
  `,
  styles: `
    :host {
      display: block;
      overflow-x: auto;
    }

    .archive-table__row--previewable {
      cursor: pointer;
    }

    .archive-table__row--previewable:hover > td,
    .archive-table__row--previewable:focus-visible > td {
      background: color-mix(in srgb, var(--color-text-muted) 10%, transparent);
    }

    .archive-table__row--previewable:focus-visible {
      outline: 2px solid var(--color-text-muted);
      outline-offset: -2px;
    }

    .archive-table__name {
      font-weight: 700;
      overflow-wrap: anywhere;
    }

    .archive-table__preview-icon {
      margin-left: 0.5rem;
      color: var(--color-text-muted);
      font-size: 1.2rem;
      vertical-align: -0.2rem;
    }

    .archive-table__date-column,
    .archive-table__download-column {
      width: 1%;
      white-space: nowrap;
    }

    time {
      color: var(--color-text-muted);
      white-space: nowrap;
    }

    .archive-table__download {
      width: fit-content;
    }

    .archive-table__download-icon {
      font-size: 1.1rem;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArchiveListComponent {
  readonly items = input.required<ArchiveItemDto[]>();

  protected readonly downloadItemsByItem = computed(
    () =>
      new Map(
        this.items().map((item) => [item, this.createDownloadItems(item)]),
      ),
  );

  private readonly messageService = inject(MessageService);

  protected file(
    item: ArchiveItemDto,
    type: ArchiveFileType,
  ): ArchiveFileDto | undefined {
    return item.files.find((file) => file.type === type);
  }

  protected pdfFile(item: ArchiveItemDto): ArchiveFileDto | undefined {
    return this.file(item, "pdf");
  }

  protected previewPdf(item: ArchiveItemDto, event?: Event): void {
    const pdf = this.pdfFile(item);
    if (!pdf) {
      return;
    }

    event?.preventDefault();
    globalThis.open(pdf.previewUrl, "_blank", "noopener,noreferrer");
  }

  protected downloadPdf(item: ArchiveItemDto): void {
    const pdf = this.pdfFile(item);
    if (pdf) {
      this.download(pdf.downloadUrl, pdf.name);
      return;
    }

    const xml = this.file(item, "xml");
    if (xml) {
      this.download(xml.downloadUrl, xml.name);
      this.messageService.add({
        severity: "info",
        summary: "XML downloaded",
        detail: "No PDF is available, so the XML file was downloaded instead.",
      });
    }
  }

  private createDownloadItems(item: ArchiveItemDto): MenuItem[] {
    const pdf = this.pdfFile(item);
    const xml = this.file(item, "xml");

    return [
      {
        label: "PDF",
        icon: "pi pi-download",
        disabled: !pdf,
        url: pdf?.downloadUrl,
      },
      {
        label: "XML",
        icon: "pi pi-download",
        disabled: !xml,
        url: xml?.downloadUrl,
      },
      {
        label: "ZIP",
        icon: "pi pi-download",
        url: item.zipDownloadUrl,
      },
    ];
  }

  private download(url: string, fileName: string): void {
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.append(link);
    link.click();
    link.remove();
  }
}
