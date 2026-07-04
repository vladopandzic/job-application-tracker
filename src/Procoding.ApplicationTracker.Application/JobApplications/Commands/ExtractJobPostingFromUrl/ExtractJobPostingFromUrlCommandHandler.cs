using FluentValidation;
using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.AiExtraction;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromUrl;

internal sealed class ExtractJobPostingFromUrlCommandHandler
    : ICommandHandler<ExtractJobPostingFromUrlCommand, ExtractedJobPostingResponseDTO>
{
    private readonly IWebPageFetcher _webPageFetcher;
    private readonly IJobPostingExtractor _extractor;

    public ExtractJobPostingFromUrlCommandHandler(IWebPageFetcher webPageFetcher, IJobPostingExtractor extractor)
    {
        _webPageFetcher = webPageFetcher;
        _extractor = extractor;
    }

    public async Task<Result<ExtractedJobPostingResponseDTO>> Handle(ExtractJobPostingFromUrlCommand request, CancellationToken cancellationToken)
    {
        var text = await _webPageFetcher.FetchReadableTextAsync(request.Url, cancellationToken);

        if (string.IsNullOrWhiteSpace(text) || text.Length < 30)
        {
            return new Result<ExtractedJobPostingResponseDTO>(
                new ValidationException("Nije moguće dohvatiti oglas s tog linka (stranica možda blokira botove). Probaj kopirati tekst oglasa."));
        }

        var extracted = await _extractor.ExtractAsync(text, cancellationToken);

        if (extracted is null)
        {
            return new Result<ExtractedJobPostingResponseDTO>(
                new ValidationException("Nije moguće izvući podatke iz oglasa. Probaj kopirati tekst oglasa."));
        }

        return new ExtractedJobPostingResponseDTO
        {
            CompanyName = extracted.CompanyName,
            PositionTitle = extracted.PositionTitle,
            JobType = extracted.JobType,
            WorkLocationType = extracted.WorkLocationType,
            Description = extracted.Description,
            CompanyWebsite = extracted.CompanyWebsite,
            JobAdLink = string.IsNullOrWhiteSpace(extracted.JobAdLink) ? request.Url : extracted.JobAdLink
        };
    }
}
