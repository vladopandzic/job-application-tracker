using FluentValidation;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Application.Candidates.Commands.ConfirmCandidateEmail;

internal sealed class ConfirmCandidateEmailCommandHandler : ICommandHandler<ConfirmCandidateEmailCommand, bool>
{
    private readonly UserManager<Candidate> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmCandidateEmailCommandHandler(UserManager<Candidate> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ConfirmCandidateEmailCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (candidate is null)
        {
            return new Result<bool>(new ValidationException("Nevažeći potvrdni link."));
        }

        // Idempotent: a second click on an already-used link should still land on "confirmed".
        if (candidate.EmailConfirmed)
        {
            return true;
        }

        var result = await _userManager.ConfirmEmailAsync(candidate, request.Token);

        if (!result.Succeeded)
        {
            return new Result<bool>(new ValidationException("Potvrdni link nije važeći ili je istekao."));
        }

        // CandidateUserStore has AutoSaveChanges = false, so ConfirmEmailAsync only tracks the change.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
