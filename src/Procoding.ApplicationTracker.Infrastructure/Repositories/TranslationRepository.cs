using Microsoft.EntityFrameworkCore;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.Infrastructure.Data;

namespace Procoding.ApplicationTracker.Infrastructure.Repositories;

internal sealed class TranslationRepository : ITranslationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TranslationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Translation>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Translations.ToListAsync(cancellationToken);
    }

    public async Task<Translation?> GetAsync(string key, string languageCode, CancellationToken cancellationToken)
    {
        return await _dbContext.Translations.FirstOrDefaultAsync(x => x.Key == key && x.LanguageCode == languageCode, cancellationToken);
    }

    public async Task InsertAsync(Translation translation, CancellationToken cancellationToken)
    {
        await _dbContext.Translations.AddAsync(translation, cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Translations.AnyAsync(cancellationToken);
    }
}
