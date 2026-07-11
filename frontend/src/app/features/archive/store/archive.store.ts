import { inject } from "@angular/core";
import { withResource } from "@angular-architects/ngrx-toolkit";
import { signalStore, withMethods, withProps } from "@ngrx/signals";

import { EtaxApiService } from "../../../core/services/etax-api.service";

export const ArchiveStore = signalStore(
  withProps(() => ({
    api: inject(EtaxApiService),
  })),
  withResource(({ api }) => api.archiveResource()),
  withMethods((store) => ({
    reload: () => store._reload(),
  })),
);
