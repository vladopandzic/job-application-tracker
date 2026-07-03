using FluentResults;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.Translations;
using Procoding.ApplicationTracker.Web.Services.Interfaces;

namespace Procoding.ApplicationTracker.Web.ViewModels.Translations;

public class TranslationsViewModel : EditViewModelBase
{
    private readonly ITranslationService _translationService;

    public TranslationsViewModel(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    public List<TranslationRow> Rows { get; set; } = new();

    public async Task InitializeViewModel(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        var result = await _translationService.GetTranslationsAsync(cancellationToken);

        IsLoading = false;

        if (result.IsSuccess)
        {
            Rows = result.Value.Translations
                .GroupBy(t => t.Key)
                .Select(g => new TranslationRow
                {
                    Key = g.Key,
                    Hr = g.FirstOrDefault(x => string.Equals(x.LanguageCode, "hr", StringComparison.OrdinalIgnoreCase))?.Value ?? "",
                    En = g.FirstOrDefault(x => string.Equals(x.LanguageCode, "en", StringComparison.OrdinalIgnoreCase))?.Value ?? ""
                })
                .OrderBy(r => r.Key)
                .ToList();
        }
    }

    /// <summary>Saves both language values for a row. Returns whether both saved.</summary>
    public async Task<Result> SaveRowAsync(TranslationRow row, CancellationToken cancellationToken = default)
    {
        IsSaving = true;

        var hr = await _translationService.UpsertTranslationAsync(new UpsertTranslationRequestDTO(row.Key, "hr", row.Hr), cancellationToken);
        var en = await _translationService.UpsertTranslationAsync(new UpsertTranslationRequestDTO(row.Key, "en", row.En), cancellationToken);

        IsSaving = false;

        return Result.Merge(hr.ToResult(), en.ToResult());
    }

    public class TranslationRow
    {
        public string Key { get; set; } = "";

        public string Hr { get; set; } = "";

        public string En { get; set; } = "";
    }
}
