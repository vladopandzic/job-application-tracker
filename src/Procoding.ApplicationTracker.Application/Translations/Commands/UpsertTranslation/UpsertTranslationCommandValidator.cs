using FluentValidation;

namespace Procoding.ApplicationTracker.Application.Translations.Commands.UpsertTranslation;

public sealed class UpsertTranslationCommandValidator : AbstractValidator<UpsertTranslationCommand>
{
    private static readonly string[] SupportedLanguages = { "hr", "en" };

    public UpsertTranslationCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty();

        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .Must(x => SupportedLanguages.Contains(x))
            .WithMessage("Language is not supported.");
    }
}
