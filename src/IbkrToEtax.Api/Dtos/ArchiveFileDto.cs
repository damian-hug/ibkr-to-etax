namespace IbkrToEtax.Dtos;

public sealed record ArchiveFileDto(
    string Name,
    string Type,
    string PreviewUrl,
    string DownloadUrl);
