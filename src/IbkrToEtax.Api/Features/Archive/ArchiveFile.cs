namespace IbkrToEtax.Features.Archive;

public sealed record ArchiveFile(
    string Name,
    string Type,
    string ContentType,
    string FullPath,
    DateTimeOffset LastModifiedAt);
