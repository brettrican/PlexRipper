using PlexRipper.Application.Common.DTO;

namespace PlexRipper.Application.Common.Interfaces;

public interface IDownloadLinkService
{
    Task<List<DownloadLinkDTO>> GetDownloadLinksAsync(
        bool includeCompleted = false,
        CancellationToken cancellationToken = default);

    Task<string> GetDownloadLinksAsTextListAsync(
        bool includeCompleted = false,
        CancellationToken cancellationToken = default);
}
