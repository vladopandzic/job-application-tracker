using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.Web.ViewModels.JobApplications;

namespace Procoding.ApplicationTracker.Web.Pages.JobApplication;

[Authorize]
public partial class ArchiveJobApplicationsPage
{
    [Inject]
    public ArchiveJobApplicationsViewModel ViewModel { get; set; } = default!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    public IDialogService DialogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await ViewModel.InitializeViewModel();
        await base.OnInitializedAsync();
    }

    private async Task Restore(Guid id)
    {
        var ok = await ViewModel.RestoreAsync(id);
        if (ok)
        {
            Snackbar.Add("Prijava vraćena iz arhive.", Severity.Success);
        }
        StateHasChanged();
    }

    private async Task Delete(JobApplicationDTO app)
    {
        var confirmed = await DialogService.ShowMessageBox(
            title: "Trajno izbrisati?",
            message: $"Prijava \"{app.JobPositionTitle}\" bit će trajno izbrisana. Ova radnja se ne može poništiti.",
            yesText: "Izbriši",
            cancelText: "Odustani");

        if (confirmed == true)
        {
            var ok = await ViewModel.DeleteAsync(app.Id);
            if (ok)
            {
                Snackbar.Add("Prijava izbrisana.", Severity.Success);
            }
            StateHasChanged();
        }
    }
}
