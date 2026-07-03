using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.Application.Translations.Commands.UpsertTranslation;

internal sealed class UpsertTranslationCommandHandler : ICommandHandler<UpsertTranslationCommand, TranslationDTO>
{
    private readonly ITranslationRepository _translationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertTranslationCommandHandler(ITranslationRepository translationRepository, IUnitOfWork unitOfWork)
    {
        _translationRepository = translationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TranslationDTO>> Handle(UpsertTranslationCommand request, CancellationToken cancellationToken)
    {
        var existing = await _translationRepository.GetAsync(request.Key, request.LanguageCode, cancellationToken);

        Translation translation;

        if (existing is null)
        {
            translation = Translation.Create(Guid.NewGuid(), request.Key, request.LanguageCode, request.Value);
            await _translationRepository.InsertAsync(translation, cancellationToken);
        }
        else
        {
            existing.UpdateValue(request.Value);
            translation = existing;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TranslationDTO(translation.Key, translation.LanguageCode, translation.Value);
    }
}
