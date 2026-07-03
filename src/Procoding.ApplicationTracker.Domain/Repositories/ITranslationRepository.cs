using Procoding.ApplicationTracker.Domain.Entities;

namespace Procoding.ApplicationTracker.Domain.Repositories;

public interface ITranslationRepository
{
    /// <summary>Gets all translations (all keys, all languages).</summary>
    Task<List<Translation>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Gets a single translation by key and language, or null.</summary>
    Task<Translation?> GetAsync(string key, string languageCode, CancellationToken cancellationToken);

    /// <summary>Inserts a new translation.</summary>
    Task InsertAsync(Translation translation, CancellationToken cancellationToken);

    /// <summary>Whether any translation exists (used to decide whether to seed).</summary>
    Task<bool> AnyAsync(CancellationToken cancellationToken);
}
