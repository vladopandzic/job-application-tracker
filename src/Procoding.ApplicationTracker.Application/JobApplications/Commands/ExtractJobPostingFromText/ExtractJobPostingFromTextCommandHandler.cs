using FluentValidation;
using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.AiExtraction;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromText;

internal sealed class ExtractJobPostingFromTextCommandHandler
    : ICommandHandler<ExtractJobPostingFromTextCommand, ExtractedJobPostingResponseDTO>
{
    private readonly IJobPostingExtractor _extractor;

    public ExtractJobPostingFromTextCommandHandler(IJobPostingExtractor extractor)
    {
        _extractor = extractor;
    }

    public async Task<Result<ExtractedJobPostingResponseDTO>> Handle(ExtractJobPostingFromTextCommand request, CancellationToken cancellationToken)
    {
        var extracted = await _extractor.ExtractAsync(request.Content, cancellationToken);

        if (extracted is null)
        {
            return new Result<ExtractedJobPostingResponseDTO>(
                new ValidationException("Nije moguće izvući podatke iz oglasa. Pokušaj zalijepiti više teksta ili je AI import trenutno nedostupan."));
        }

        return new ExtractedJobPostingResponseDTO
        {
            CompanyName = extracted.CompanyName,
            PositionTitle = extracted.PositionTitle,
            JobType = extracted.JobType,
            WorkLocationType = extracted.WorkLocationType,
            Description = extracted.Description,
            CompanyWebsite = extracted.CompanyWebsite
        };
    }
}
