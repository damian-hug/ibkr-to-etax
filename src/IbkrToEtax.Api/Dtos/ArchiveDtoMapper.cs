using IbkrToEtax.Features.Archive;

namespace IbkrToEtax.Dtos;

public static class ArchiveDtoMapper
{
    public static ArchiveItemDto Map(
        ArchiveItem item,
        Func<string, string> getFileUrl,
        Func<string, string> getZipDownloadUrl)
    {
        return new ArchiveItemDto(
            item.Name,
            item.GeneratedAt,
            getZipDownloadUrl(item.Name),
            item.Files.Select(file =>
            {
                var previewUrl = getFileUrl(file.Name);
                return new ArchiveFileDto(
                    file.Name,
                    file.Type,
                    previewUrl,
                    $"{previewUrl}/download");
            }).ToArray());
    }
}
