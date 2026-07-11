namespace IbkrToEtax.Features.Archive;

public sealed record ArchiveItem(
    string Name,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<ArchiveFile> Files);
