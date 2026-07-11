namespace IbkrToEtax.Dtos;

public sealed record ArchiveItemDto(
    string Name,
    DateTimeOffset GeneratedAt,
    string ZipDownloadUrl,
    IReadOnlyList<ArchiveFileDto> Files);
