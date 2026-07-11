using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IbkrToEtax.Controllers;
using IbkrToEtax.Dtos;
using IbkrToEtax.Features.Archive;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IbkrToEtax.Tests.Archive;

public sealed class ArchiveApiTests
{
    [Fact]
    public async Task GetArchive_ReturnsCamelCaseContractWithFileLinks()
    {
        using var archive = new TemporaryArchive();
        archive.AddFile("tax 2025.output.xml", Utc(2026, 2, 1), "<taxStatement />");
        archive.AddFile("tax 2025.output.pdf", Utc(2026, 2, 2), "%PDF-test");
        using var server = CreateServer(archive);
        using var client = server.CreateClient();

        var response = await client.GetAsync("/api/archive");

        response.EnsureSuccessStatusCode();
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement[0].TryGetProperty("name", out _));
        Assert.False(json.RootElement[0].TryGetProperty("Name", out _));

        var item = Assert.Single(
            json.RootElement.Deserialize<ArchiveItemDto[]>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!);
        Assert.Equal("tax 2025", item.Name);
        Assert.Equal(Utc(2026, 2, 2), item.GeneratedAt.UtcDateTime);
        Assert.Equal(2, item.Files.Count);
        Assert.Equal(
            "/api/archive/files/tax%202025.output.xml",
            item.Files[0].PreviewUrl);
        Assert.Equal(
            "/api/archive/files/tax%202025.output.xml/download",
            item.Files[0].DownloadUrl);
    }

    [Fact]
    public async Task PreviewAndDownload_ReturnExpectedContentHeaders()
    {
        using var archive = new TemporaryArchive();
        archive.AddFile("statement.output.pdf", Utc(2026, 2, 1), "%PDF-test");
        using var server = CreateServer(archive);
        using var client = server.CreateClient();

        var preview = await client.GetAsync("/api/archive/files/statement.output.pdf");
        var download = await client.GetAsync("/api/archive/files/statement.output.pdf/download");

        preview.EnsureSuccessStatusCode();
        Assert.Equal("application/pdf", preview.Content.Headers.ContentType?.MediaType);
        Assert.Null(preview.Content.Headers.ContentDisposition);

        download.EnsureSuccessStatusCode();
        Assert.Equal("application/pdf", download.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", download.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(
            "statement.output.pdf",
            download.Content.Headers.ContentDisposition?.FileNameStar);
    }

    [Fact]
    public async Task FileEndpoints_ReturnNotFoundForMissingOrUnsupportedFiles()
    {
        using var archive = new TemporaryArchive();
        using var server = CreateServer(archive);
        using var client = server.CreateClient();

        var missing = await client.GetAsync("/api/archive/files/missing.output.xml");
        var unsupported = await client.GetAsync("/api/archive/files/debug.pdf/download");

        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unsupported.StatusCode);
    }

    private static TestServer CreateServer(TemporaryArchive archive)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new ArchiveOptions
                {
                    OutputDirectory = archive.OutputDirectory
                });
                services.AddSingleton<IArchiveService, ArchiveService>();
                services
                    .AddControllers()
                    .AddApplicationPart(typeof(ArchiveController).Assembly);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            });

        return new TestServer(builder);
    }

    private static DateTime Utc(int year, int month, int day) =>
        new(year, month, day, 12, 0, 0, DateTimeKind.Utc);
}
