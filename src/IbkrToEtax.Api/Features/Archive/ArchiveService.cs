namespace IbkrToEtax.Features.Archive;

public sealed class ArchiveService : IArchiveService
{
    private const string OutputSuffix = ".output";

    private readonly string _outputDirectory;

    public ArchiveService(ArchiveOptions options)
    {
        _outputDirectory = Path.GetFullPath(options.OutputDirectory);
    }

    public IReadOnlyList<ArchiveItem> GetItems()
    {
        if (!Directory.Exists(_outputDirectory))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(_outputDirectory, "*", SearchOption.TopDirectoryOnly)
            .Select(CreateArchiveFile)
            .Where(file => file is not null)
            .Cast<(string ItemName, ArchiveFile File)>()
            .GroupBy(entry => entry.ItemName, StringComparer.Ordinal)
            .Select(group => new ArchiveItem(
                group.Key,
                group.Max(entry => entry.File.LastModifiedAt),
                group
                    .Select(entry => entry.File)
                    .OrderBy(file => file.Type == "xml" ? 0 : 1)
                    .ToArray()))
            .OrderByDescending(item => item.GeneratedAt)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Name, StringComparer.Ordinal)
            .ToArray();
    }

    public ArchiveFile? GetFile(string fileName)
    {
        if (!TryGetFileMetadata(fileName, out var itemName, out var type, out var contentType))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(Path.Combine(_outputDirectory, fileName));
        if (!IsInsideOutputDirectory(fullPath) || !File.Exists(fullPath))
        {
            return null;
        }

        var fileInfo = new FileInfo(fullPath);
        return new ArchiveFile(
            fileInfo.Name,
            type,
            contentType,
            fileInfo.FullName,
            fileInfo.LastWriteTimeUtc);
    }

    public string GetFileUrl(string fileName) =>
        $"/api/archive/files/{Uri.EscapeDataString(fileName)}";

    private (string ItemName, ArchiveFile File)? CreateArchiveFile(string path)
    {
        var fileName = Path.GetFileName(path);
        if (!TryGetFileMetadata(fileName, out var itemName, out var type, out var contentType))
        {
            return null;
        }

        var fileInfo = new FileInfo(path);
        return (
            itemName,
            new ArchiveFile(
                fileInfo.Name,
                type,
                contentType,
                fileInfo.FullName,
                fileInfo.LastWriteTimeUtc));
    }

    private bool IsInsideOutputDirectory(string fullPath)
    {
        var parentDirectory = Path.GetDirectoryName(fullPath);
        return string.Equals(
            parentDirectory,
            _outputDirectory,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
    }

    private static bool TryGetFileMetadata(
        string fileName,
        out string itemName,
        out string type,
        out string contentType)
    {
        itemName = string.Empty;
        type = string.Empty;
        contentType = string.Empty;

        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        if (string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
        {
            type = "xml";
            contentType = "application/xml";
        }
        else if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            type = "pdf";
            contentType = "application/pdf";
        }
        else
        {
            return false;
        }

        itemName = fileName[..^extension.Length];
        if (itemName.EndsWith(OutputSuffix, StringComparison.OrdinalIgnoreCase))
        {
            itemName = itemName[..^OutputSuffix.Length];
        }

        return itemName.Length > 0;
    }
}
