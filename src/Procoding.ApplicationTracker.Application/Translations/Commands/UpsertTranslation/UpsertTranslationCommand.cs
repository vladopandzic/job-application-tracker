using LanguageExt.Common;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.Application.Translations.Commands.UpsertTranslation;

public sealed class UpsertTranslationCommand : ICommand<Result<TranslationDTO>>
{
    public UpsertTranslationCommand(string key, string languageCode, string value)
    {
        Key = key;
        LanguageCode = languageCode;
        Value = value;
    }

    public string Key { get; }

    public string LanguageCode { get; }

    public string Value { get; }
}
