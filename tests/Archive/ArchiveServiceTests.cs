using IbkrToEtax.Features.Archive;
using Xunit;

namespace IbkrToEtax.Tests.Archive;

public sealed class ArchiveServiceTests
{
    [Fact]
    public void GetItems_GroupsPairsAndKeepsIncompleteConversions()
    {
        using var archive = new TemporaryArchive();
        archive.AddFile("complete.output.xml", Utc(2026, 1, 1));
        archive.AddFile("complete.output.pdf", Utc(2026, 1, 2));
        archive.AddFile("xml-only.output.xml", Utc(2026, 1, 3));

        var items = CreateService(archive).GetItems();

        Assert.Equal(2, items.Count);
        var complete = Assert.Single(items, item => item.Name == "complete");
        Assert.Equal(Utc(2026, 1, 2), complete.GeneratedAt);
        Assert.Collection(
            complete.Files,
            file => Assert.Equal("xml", file.Type),
            file => Assert.Equal("pdf", file.Type));

        var incomplete = Assert.Single(items, item => item.Name == "xml-only");
        Assert.Single(incomplete.Files);
        Assert.Equal("xml", incomplete.Files[0].Type);
    }

    [Fact]
    public void GetItems_IncludesAllPdfAndXmlFiles()
    {
        using var archive = new TemporaryArchive();
        archive.AddFile("included.output.xml", Utc(2026, 1, 1));
        archive.AddFile("debug.pdf", Utc(2026, 1, 2));
        archive.AddFile("validated-extracted.xml", Utc(2026, 1, 3));
        archive.AddFile("notes.txt", Utc(2026, 1, 4));

        var names = CreateService(archive).GetItems().Select(item => item.Name).ToArray();

        Assert.Equal(
            new[] { "validated-extracted", "debug", "included" },
            names);
    }

    [Fact]
    public void GetItems_SortsNewestFirstThenByName()
    {
        using var archive = new TemporaryArchive();
        archive.AddFile("older.output.xml", Utc(2026, 1, 1));
        archive.AddFile("bravo.output.pdf", Utc(2026, 1, 2));
        archive.AddFile("Alpha.output.xml", Utc(2026, 1, 2));

        var names = CreateService(archive).GetItems().Select(item => item.Name).ToArray();

        Assert.Collection(
            names,
            name => Assert.Equal("Alpha", name),
            name => Assert.Equal("bravo", name),
            name => Assert.Equal("older", name));
    }

    [Theory]
    [InlineData("../secret.output.xml")]
    [InlineData("notes.txt")]
    [InlineData(".output.xml")]
    public void GetFile_RejectsUnsafeOrUnsupportedNames(string fileName)
    {
        using var archive = new TemporaryArchive();

        Assert.Null(CreateService(archive).GetFile(fileName));
    }

    [Theory]
    [InlineData("ordinary.xml", "application/xml")]
    [InlineData("debug.pdf", "application/pdf")]
    [InlineData("UPPER.XML", "application/xml")]
    public void GetFile_AcceptsAnyPdfOrXmlName(string fileName, string contentType)
    {
        using var archive = new TemporaryArchive();
        archive.AddFile(fileName, Utc(2026, 1, 1));

        var file = CreateService(archive).GetFile(fileName);

        Assert.NotNull(file);
        Assert.Equal(contentType, file.ContentType);
    }

    [Fact]
    public void GetFile_ReturnsSupportedFileMetadataAndMissingFilesReturnNull()
    {
        using var archive = new TemporaryArchive();
        var path = archive.AddFile("statement.output.pdf", Utc(2026, 1, 1));
        var service = CreateService(archive);

        var file = service.GetFile("statement.output.pdf");

        Assert.NotNull(file);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal(path, file.FullPath);

        File.Delete(path);
        Assert.Null(service.GetFile("statement.output.pdf"));
    }

    [Fact]
    public void GetFileUrl_ReturnsEncodedArchiveRoute()
    {
        using var archive = new TemporaryArchive();

        var url = CreateService(archive).GetFileUrl("tax 2025.output.xml");

        Assert.Equal("/api/archive/files/tax%202025.output.xml", url);
    }

    private static ArchiveService CreateService(TemporaryArchive archive) =>
        new(new ArchiveOptions { OutputDirectory = archive.OutputDirectory });

    private static DateTime Utc(int year, int month, int day) =>
        new(year, month, day, 12, 0, 0, DateTimeKind.Utc);
}
