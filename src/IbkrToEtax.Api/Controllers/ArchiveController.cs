using System.IO.Compression;
using IbkrToEtax.Dtos;
using IbkrToEtax.Features.Archive;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IbkrToEtax.Controllers;

[ApiController]
[Route("api/archive")]
public sealed class ArchiveController(IArchiveService archiveService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArchiveItemDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ArchiveItemDto>> GetItems()
    {
        var items = archiveService
            .GetItems()
            .Select(item => ArchiveDtoMapper.Map(
                item,
                archiveService.GetFileUrl,
                itemName =>
                    $"/api/archive/items/{Uri.EscapeDataString(itemName)}/download"))
            .ToArray();

        return Ok(items);
    }

    [HttpGet("files/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Preview(string fileName)
    {
        var file = archiveService.GetFile(fileName);
        return file is null
            ? NotFound()
            : PhysicalFile(file.FullPath, file.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("files/{fileName}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Download(string fileName)
    {
        var file = archiveService.GetFile(fileName);
        return file is null
            ? NotFound()
            : PhysicalFile(
                file.FullPath,
                file.ContentType,
                file.Name,
                enableRangeProcessing: true);
    }

    [HttpGet("items/{itemName}/download")]
    [Produces("application/zip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DownloadItem(string itemName)
    {
        var item = archiveService
            .GetItems()
            .SingleOrDefault(candidate =>
                string.Equals(candidate.Name, itemName, StringComparison.Ordinal));

        if (item is null)
        {
            return NotFound();
        }

        var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in item.Files)
            {
                archive.CreateEntryFromFile(
                    file.FullPath,
                    file.Name,
                    CompressionLevel.Fastest);
            }
        }

        output.Position = 0;
        return File(output, "application/zip", $"{item.Name}.zip");
    }
}
