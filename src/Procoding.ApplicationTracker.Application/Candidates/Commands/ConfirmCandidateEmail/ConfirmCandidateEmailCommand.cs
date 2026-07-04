using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;

namespace Procoding.ApplicationTracker.Application.Candidates.Commands.ConfirmCandidateEmail;

public sealed class ConfirmCandidateEmailCommand : ICommand<Result<bool>>
{
    public ConfirmCandidateEmailCommand(Guid userId, string token)
    {
        UserId = userId;
        Token = token;
    }

    public Guid UserId { get; }

    public string Token { get; }
}
