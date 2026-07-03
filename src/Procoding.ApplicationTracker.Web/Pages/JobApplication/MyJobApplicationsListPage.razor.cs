using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.Web.Localization;
using Procoding.ApplicationTracker.Web.ViewModels.JobApplications;

namespace Procoding.ApplicationTracker.Web.Pages.JobApplication;

public partial class MyJobApplicationsListPage : IDisposable
{
    [Inject]
    public JobApplicationListViewModel ViewModel { get; set; } = default!;

    [Inject]
    public LocalizationService Loc { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await Loc.EnsureLoadedAsync();
        Loc.OnChange += OnLocalizationChanged;
        await base.OnInitializedAsync();
    }

    private void OnLocalizationChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Loc.OnChange -= OnLocalizationChanged;

    private static string StatusKey(string status) => status switch
    {
        "Applied" => "status.applied",
        "InProcess" => "status.inProcess",
        "Offer" => "status.offer",
        "Accepted" => "status.accepted",
        "Rejected" => "status.rejected",
        "Withdrawed" => "status.withdrawed",
        _ => status
    };

    private async Task<GridData<JobApplicationDTO>> LoadGridData(GridState<JobApplicationDTO> state)
    {

        ViewModel.Request = GridStateConverter.ConvertToRequest<JobApplicationGetListRequestDTO, JobApplicationDTO>(state);

        await ViewModel.GetJobApplications();

        GridData<JobApplicationDTO> data = new GridData<JobApplicationDTO>
        {
            Items = ViewModel.JobApplications,
            TotalItems = ViewModel.TotalNumberOfJobApplications
        };

        // The mobile card list reads ViewModel.JobApplications directly (it's not inside the grid),
        // so re-render the page once the grid's data load completes.
        await InvokeAsync(StateHasChanged);

        return data;
    }

    private static Color StatusColor(string status) => status switch
    {
        "Applied" => Color.Info,
        "InProcess" => Color.Warning,
        "Offer" => Color.Secondary,
        "Accepted" => Color.Success,
        "Rejected" => Color.Error,
        _ => Color.Default
    };
}
