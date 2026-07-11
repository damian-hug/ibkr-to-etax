namespace IbkrToEtax.Features.Archive;

public interface IArchiveService
{
    IReadOnlyList<ArchiveItem> GetItems();

    ArchiveFile? GetFile(string fileName);

    string GetFileUrl(string fileName);
}
