using System.Text;
using PlexRipper.Application.Common.DTO;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.Services;

public class DownloadLinkService : IDownloadLinkService
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ILog _log;

    public DownloadLinkService(IPlexRipperDbContext dbContext, ILog log)
    {
        _dbContext = dbContext;
        _log = log;
    }

    public async Task<List<DownloadLinkDTO>> GetDownloadLinksAsync(bool includeCompleted = false, CancellationToken cancellationToken = default)
    {
        var result = new List<DownloadLinkDTO>();
        
        // Get all download tasks that are either queued, downloading, or completed (if includeCompleted is true)
        var statuses = new List<DownloadStatus>
        {
            DownloadStatus.Queued,
            DownloadStatus.Downloading,
            DownloadStatus.Merging,
            DownloadStatus.Moving
        };

        if (includeCompleted)
        {
            statuses.Add(DownloadStatus.Completed);
        }

        // Get movie downloads
        var movieFiles = await _dbContext.DownloadTaskMovieFile
            .Where(x => statuses.Contains(x.DownloadStatus))
            .ToListAsync(cancellationToken);

        // Get TV show episode downloads
        var episodeFiles = await _dbContext.DownloadTaskTvShowEpisodeFile
            .Where(x => statuses.Contains(x.DownloadStatus))
            .ToListAsync(cancellationToken);

        // Process movie downloads
        foreach (var movie in movieFiles)
        {
            var downloadUrl = await _dbContext.GetDownloadUrl(movie.PlexServerId, movie.FileLocationUrl, cancellationToken);
            if (downloadUrl.IsFailed)
            {
                _log.Warning("Failed to get download URL for movie {MovieTitle} (ID: {MovieId})", movie.FileName, movie.Id);
                continue;
            }

            result.Add(new DownloadLinkDTO
            {
                Id = movie.Id,
                Title = movie.FileName,
                FileName = movie.FileName,
                DownloadUrl = downloadUrl.Value,
                FileSize = movie.DataTotal,
                Status = movie.DownloadStatus,
                MediaType = PlexMediaType.Movie
            });
        }

        // Process TV show episode downloads
        foreach (var episode in episodeFiles)
        {
            var downloadUrl = await _dbContext.GetDownloadUrl(episode.PlexServerId, episode.FileLocationUrl, cancellationToken);
            if (downloadUrl.IsFailed)
            {
                _log.Warning("Failed to get download URL for episode {EpisodeTitle} (ID: {EpisodeId})", episode.FileName, episode.Id);
                continue;
            }

            result.Add(new DownloadLinkDTO
            {
                Id = episode.Id,
                Title = episode.FileName,
                FileName = episode.FileName,
                DownloadUrl = downloadUrl.Value,
                FileSize = episode.DataTotal,
                Status = episode.DownloadStatus,
                MediaType = PlexMediaType.Episode
            });
        }

        return result;
    }

    public async Task<string> GetDownloadLinksAsTextListAsync(bool includeCompleted = false, CancellationToken cancellationToken = default)
    {
        var links = await GetDownloadLinksAsync(includeCompleted, cancellationToken);
        var sb = new StringBuilder();
        
        foreach (var link in links)
        {
            sb.AppendLine(link.DownloadUrl);
        }
        
        return sb.ToString();
    }
}
