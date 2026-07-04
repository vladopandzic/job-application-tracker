using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Procoding.ApplicationTracker.Application.Core.Abstractions.AiExtraction;

namespace Procoding.ApplicationTracker.Infrastructure.AiExtraction;

/// <summary>
/// Fetches a job posting URL and strips it down to readable text for the AI extractor. Best-effort:
/// returns null on invalid URL, non-success status, or any exception.
/// </summary>
internal sealed class WebPageFetcher : IWebPageFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebPageFetcher> _logger;

    public WebPageFetcher(HttpClient httpClient, ILogger<WebPageFetcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> FetchReadableTextAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            // A browser-ish UA gets past the most trivial bot checks; big boards still block us — that's expected.
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; JobTrekBot/1.0; +https://jobtrek.runasp.net)");
            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("URL fetch for {Url} returned {Status}.", uri, response.StatusCode);
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            return StripHtml(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch URL {Url}.", uri);
            return null;
        }
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        html = Regex.Replace(html, "<script.*?</script>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        html = Regex.Replace(html, "<style.*?</style>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        html = Regex.Replace(html, "<!--.*?-->", " ", RegexOptions.Singleline);
        html = Regex.Replace(html, "<[^>]+>", " ");
        html = WebUtility.HtmlDecode(html);
        html = Regex.Replace(html, "\\s+", " ").Trim();

        return html;
    }
}
