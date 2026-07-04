using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromUrl;

public sealed class ExtractJobPostingFromUrlCommand : ICommand<Result<ExtractedJobPostingResponseDTO>>
{
    public ExtractJobPostingFromUrlCommand(string url)
    {
        Url = url;
    }

    public string Url { get; }
}
