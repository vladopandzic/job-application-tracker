using FluentValidation;

namespace Procoding.ApplicationTracker.Web.Validators;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(BeAValidUrl)
                         .WithMessage("Unesi ispravnu poveznicu (npr. www.tvrtka.com).");
    }

    /// <summary>
    /// Accepts links with or without a scheme (e.g. "www.tvrtka.com", "tvrtka.com/posao",
    /// "https://tvrtka.com"). The stored value is normalized with <see cref="NormalizeUrl"/> on save.
    /// </summary>
    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true; // NotEmpty handles the empty case
        }

        var candidate = url.Trim();
        if (!candidate.Contains("://"))
        {
            candidate = "https://" + candidate;
        }

        return Uri.TryCreate(candidate, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps)
               && result.Host.Contains('.');
    }

    /// <summary>Prepends https:// if the user omitted the scheme, so stored links are clickable.</summary>
    public static string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        var trimmed = url.Trim();
        return trimmed.Contains("://") ? trimmed : "https://" + trimmed;
    }
}
