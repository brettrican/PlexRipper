using PlexRipper.Domain;

namespace PlexRipper.Application.Common.DTO;

public class DownloadLinkDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DownloadStatus Status { get; set; }
    public PlexMediaType MediaType { get; set; }
}
