import { HttpResourceRef, httpResource } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { ArchiveItemDto } from "./etax-api.models";

@Injectable({ providedIn: "root" })
export class EtaxApiService {
  archiveResource(): HttpResourceRef<ArchiveItemDto[]> {
    return httpResource<ArchiveItemDto[]>(() => "/api/archive", {
      defaultValue: [],
    });
  }
}
