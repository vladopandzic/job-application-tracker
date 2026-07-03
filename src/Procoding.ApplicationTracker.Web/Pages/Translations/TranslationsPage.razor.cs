using Microsoft.AspNetCore.Components;
using MudBlazor;
using Procoding.ApplicationTracker.Web.Localization;
using Procoding.ApplicationTracker.Web.ViewModels.Translations;

namespace Procoding.ApplicationTracker.Web.Pages.Translations;

public partial class TranslationsPage
{
    [Inject]
    public TranslationsViewModel ViewModel { get; set; } = default!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    public LocalizationService Loc { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await ViewModel.InitializeViewModel();
        await base.OnInitializedAsync();
    }

    private async Task SaveRow(TranslationsViewModel.TranslationRow row)
    {
        var result = await ViewModel.SaveRowAsync(row);

        if (result.IsSuccess)
        {
            // Reflect the change immediately in this session's cached translations.
            Loc.ReplaceTranslation(row.Key, "hr", row.Hr);
            Loc.ReplaceTranslation(row.Key, "en", row.En);
            Snackbar.Add($"Saved '{row.Key}'.", Severity.Success);
        }
        else
        {
            Snackbar.Add(string.Join(" ", result.Errors.Select(e => e.Message)), Severity.Error);
        }
    }
}
