using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Common;

namespace Procoding.ApplicationTracker.Domain.Entities;

/// <summary>
/// A translatable UI string: a <see cref="Key"/> has one value per <see cref="LanguageCode"/>.
/// Editable by admins so copy can be changed without a deploy.
/// </summary>
public sealed class Translation : AggregateRoot, IAuditableEntity
{
    public static readonly int MaxLengthForKey = 200;
    public static readonly int MaxLengthForLanguageCode = 10;
    public static readonly int MaxLengthForValue = 2000;

#pragma warning disable CS8618
    private Translation()
    {
    } //used by EF core
#pragma warning restore CS8618

    private Translation(Guid id, string key, string languageCode, string value) : base(id)
    {
        Key = key;
        LanguageCode = languageCode;
        Value = value;
    }

    public static Translation Create(Guid id, string key, string languageCode, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        return new Translation(id, key, languageCode, value ?? string.Empty);
    }

    public void UpdateValue(string value)
    {
        Value = value ?? string.Empty;
    }

    /// <summary>Translation key, e.g. "nav.home".</summary>
    public string Key { get; private set; }

    /// <summary>Language code, e.g. "hr" or "en".</summary>
    public string LanguageCode { get; private set; }

    /// <summary>Translated text.</summary>
    public string Value { get; private set; }

    /// <inheritdoc/>
    public DateTime CreatedOnUtc { get; private set; }

    /// <inheritdoc/>
    public DateTime ModifiedOnUtc { get; private set; }
}
