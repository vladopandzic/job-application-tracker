namespace Procoding.ApplicationTracker.Application.Core.Abstractions.AiExtraction;

/// <summary>
/// Fetches a web page and returns its readable text (tags stripped). Implemented in Infrastructure.
/// Returns null on any failure (bad URL, blocked by the site, non-success status) so callers degrade
/// gracefully — many big job boards (LinkedIn, Indeed) block bots, so URL import is best-effort.
/// </summary>
public interface IWebPageFetcher
{
    Task<string?> FetchReadableTextAsync(string url, CancellationToken cancellationToken = default);
}
