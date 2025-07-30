using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application.Common.DTO;
using PlexRipper.Application.Common.Interfaces;

namespace PlexRipper.WebAPI.Features.DownloadLinks;

/// <summary>
/// Controller for managing download links
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DownloadLinksController : ControllerBase
{
    private readonly IDownloadLinkService _downloadLinkService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadLinksController"/> class
    /// </summary>
    /// <param name="downloadLinkService">The download link service</param>
    public DownloadLinksController(IDownloadLinkService downloadLinkService)
    {
        _downloadLinkService = downloadLinkService;
    }

    /// <summary>
    /// Get all download links as a list of DTOs
    /// </summary>
    /// <param name="includeCompleted">Whether to include completed downloads</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of download links</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<DownloadLinkDTO>), 200)]
    public async Task<IActionResult> GetDownloadLinks(
        [FromQuery] bool includeCompleted = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _downloadLinkService.GetDownloadLinksAsync(includeCompleted, cancellationToken);
        return new OkObjectResult(result);
    }

    /// <summary>
    /// Get all download links as a plain text list (one URL per line)
    /// </summary>
    /// <param name="includeCompleted">Whether to include completed downloads</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Plain text list of download URLs</returns>
    [HttpGet("text")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), 200)]
    public async Task<IActionResult> GetDownloadLinksAsText(
        [FromQuery] bool includeCompleted = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _downloadLinkService.GetDownloadLinksAsTextListAsync(includeCompleted, cancellationToken);
        return Content(result, "text/plain");
    }

    /// <summary>
    /// Get download links as a downloadable text file
    /// </summary>
    /// <param name="includeCompleted">Whether to include completed downloads</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Text file with download URLs</returns>
    [HttpGet("download")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> DownloadLinksFile(
        [FromQuery] bool includeCompleted = false,
        CancellationToken cancellationToken = default)
    {
        var content = await _downloadLinkService.GetDownloadLinksAsTextListAsync(includeCompleted, cancellationToken);
        var fileName = $"plex_download_links_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
        
        Response.Headers["Content-Disposition"] = $"attachment; filename={fileName}";
        return Content(content, "text/plain");
    }
}
