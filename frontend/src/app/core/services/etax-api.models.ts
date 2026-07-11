export type ArchiveFileType = "xml" | "pdf";

export interface ArchiveFileDto {
  readonly name: string;
  readonly type: ArchiveFileType;
  readonly previewUrl: string;
  readonly downloadUrl: string;
}

export interface ArchiveItemDto {
  readonly name: string;
  readonly generatedAt: string;
  readonly zipDownloadUrl: string;
  readonly files: readonly ArchiveFileDto[];
}
