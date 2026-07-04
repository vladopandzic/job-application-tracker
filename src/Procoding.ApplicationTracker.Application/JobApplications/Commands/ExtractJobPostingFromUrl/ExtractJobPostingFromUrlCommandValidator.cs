using FluentValidation;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromUrl;

public sealed class ExtractJobPostingFromUrlCommandValidator : AbstractValidator<ExtractJobPostingFromUrlCommand>
{
    public ExtractJobPostingFromUrlCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Unesi ispravan link (npr. https://…).");
    }
}
