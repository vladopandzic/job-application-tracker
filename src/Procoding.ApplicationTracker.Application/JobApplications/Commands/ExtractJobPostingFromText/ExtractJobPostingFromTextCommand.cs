using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Application.JobApplications.Commands.ExtractJobPostingFromText;

public sealed class ExtractJobPostingFromTextCommand : ICommand<Result<ExtractedJobPostingResponseDTO>>
{
    public ExtractJobPostingFromTextCommand(string content)
    {
        Content = content;
    }

    public string Content { get; }
}
