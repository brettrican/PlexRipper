using System.Text;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlexRipper.Application.Common.DTO;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Data;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.Services;

public class DownloadLinkService : IDownloadLinkService
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ILogger _log;

    public DownloadLinkService(
        IPlexRipperDbContext dbContext, 
        ILogger<DownloadLinkService> log)
    {
        _dbContext = dbContext;
        _log = log;
    }

    public async Task<List<DownloadLinkDTO>> GetDownloadLinksAsync(
        bool includeCompleted = false, 
        CancellationToken cancellationToken = default)
    {
        var statuses = new List<DownloadStatus>
        {
            DownloadStatus.Queued,
            DownloadStatus.Downloading,
            DownloadStatus.Merging,
            DownloadStatus.Moving
        };

        if (includeCompleted)
        {
            statuses.AddRange(new[]
            {
                DownloadStatus.DownloadFinished,
                DownloadStatus.MergeFinished,
                DownloadStatus.MoveFinished,
                DownloadStatus.Completed
            });
        }

        // Get movie files
        var movieFiles = await _dbContext.DownloadTaskMovieFile
            .Include(x => x.PlexServer)
            .Include(x => x.PlexLibrary)
            .Where(x => statuses.Contains(x.DownloadStatus))
            .ToListAsync(cancellationToken);

        // Get TV show episode files
        var episodeFiles = await _dbContext.DownloadTaskTvShowEpisodeFile
            .Include(x => x.PlexServer)
            .Include(x => x.PlexLibrary)
            .Where(x => statuses.Contains(x.DownloadStatus))
            .ToListAsync(cancellationToken);

        var result = new List<DownloadLinkDTO>();

        // Process movie files
        foreach (var movieFile in movieFiles)
        {
            var urlResult = await _dbContext.GetDownloadUrl(
                movieFile.PlexServerId,
                movieFile.FileLocationUrl, cancellationToken);

            if (urlResult.IsSuccess)
            {
                result.Add(new DownloadLinkDTO
                {
                    Title = movieFile.Title,
                    Url = urlResult.Value,
                    Type = "Movie",
                    Status = movieFile.DownloadStatus.ToString()
                });
            }
        }

        // Process TV show episodes
        foreach (var episodeFile in episodeFiles)
        {
            var urlResult = await _dbContext.GetDownloadUrl(
                episodeFile.PlexServerId,
                episodeFile.FileLocationUrl, cancellationToken);

            if (urlResult.IsSuccess)
            {
                result.Add(new DownloadLinkDTO
                {
                    Title = episodeFile.FullTitle,
                    Url = urlResult.Value,
                    Type = "TV Show",
                    Status = episodeFile.DownloadStatus.ToString()
                });
            }
        }

        return result;
    }

    public async Task<string> GetDownloadLinksAsTextListAsync(bool includeCompleted = false, CancellationToken cancellationToken = default)
    {
        var links = await GetDownloadLinksAsync(includeCompleted, cancellationToken);
        var sb = new StringBuilder();
        
        foreach (var link in links)
        {
            sb.AppendLine($"# {link.Type}: {link.Title}");
            sb.AppendLine(link.Url);
            sb.AppendLine();
        }
        
        return sb.ToString().TrimEnd();
    }
}
