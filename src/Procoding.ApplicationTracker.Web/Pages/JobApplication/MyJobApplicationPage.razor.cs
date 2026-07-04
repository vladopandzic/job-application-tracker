using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.Web.Localization;
using Procoding.ApplicationTracker.Web.ViewModels.JobApplications;

namespace Procoding.ApplicationTracker.Web.Pages.JobApplication;

[Authorize]
public partial class MyJobApplicationPage : IDisposable
{
    [Inject]
    public MyJobApplicationViewModel ViewModel { get; set; } = default!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    public IDialogService DialogService { get; set; } = default!;

    [Inject]
    public LocalizationService Loc { get; set; } = default!;

    [Parameter]
    public string Id { get; set; }

    // Fixed-choice fields are MudSelects; their DTOs use reference equality, so compare by value/id
    // for the selected item to match a list item.
    private static readonly IEqualityComparer<WorkLocationTypeDTO> WorkLocationComparer =
        EqualityComparer<WorkLocationTypeDTO>.Create((a, b) => string.Equals(a?.Value, b?.Value, StringComparison.Ordinal), x => x.Value?.GetHashCode() ?? 0);

    private static readonly IEqualityComparer<JobTypeDTO> JobTypeComparer =
        EqualityComparer<JobTypeDTO>.Create((a, b) => string.Equals(a?.Value, b?.Value, StringComparison.Ordinal), x => x.Value?.GetHashCode() ?? 0);

    private static readonly IEqualityComparer<JobApplicationSourceDTO> SourceComparer =
        EqualityComparer<JobApplicationSourceDTO>.Create((a, b) => a?.Id == b?.Id, x => x.Id.GetHashCode());

    protected MudForm? mudForm;

    private MudDialog dialogRef = default!;

    private MudForm? newNewCompanyForm = default!;

    private MudAutocomplete<CompanyDTO> mudCompanyAutocomplete = default!;

    protected override async Task OnParametersSetAsync()
    {
        if (ViewModel.JobApplication?.Id != null && Id == ViewModel.JobApplication?.Id.ToString())
        {
            return;
        }
        if (Id == "new")
        {
            await ViewModel.InitializeViewModel(null);
        }
        else
        {
            await ViewModel.InitializeViewModel(Guid.Parse(Id));
        }
        await base.OnParametersSetAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await Loc.EnsureLoadedAsync();
        Loc.OnChange += OnLocalizationChanged;

        if (Id == "new")
        {
            await ViewModel.InitializeViewModel(null);
        }
        else
        {
            await ViewModel.InitializeViewModel(Guid.Parse(Id));
        }
        await base.OnInitializedAsync();
    }

    private void OnLocalizationChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Loc.OnChange -= OnLocalizationChanged;

    private async Task SaveNewCompany()
    {
        await newNewCompanyForm!.Validate();
        await ViewModel.SaveNewCompany();

        if (ViewModel.JobApplication?.Company?.Id != Guid.Empty)
        {
            await mudCompanyAutocomplete.SelectOption(ViewModel.JobApplication!.Company);
        }
    }

    protected async Task OnSubmit()
    {
        await mudForm!.Validate();

        if (!mudForm.IsValid)
        {
            return;
        }

        await ViewModel.SaveAsync();
        ViewModel.EditDetailsDialogVisible = false;
    }

    private void OpenEditDetails() => ViewModel.EditDetailsDialogVisible = true;

    private async Task ImportFromAi()
    {
        var error = await ViewModel.ImportFromTextAsync();

        if (error is not null)
        {
            Snackbar.Add(error, Severity.Error);
        }
        else
        {
            Snackbar.Add("Polja su popunjena AI-jem — pregledaj i spremi.", Severity.Success);
        }

        StateHasChanged();
    }

    private string CompaniesToStringFunc(CompanyDTO company)
    {
        return company?.CompanyName ?? string.Empty;
    }

    private string CandidatesToStringFunc(CandidateDTO candidate)
    {
        return candidate != null ? candidate.Name + " " + candidate.Surname : "";
    }

    private string JobApplicationSourcesToStringFunc(JobApplicationSourceDTO jobApplicationSource)
    {
        return jobApplicationSource != null ? jobApplicationSource.Name : "";
    }

    private string JobTypeToStringFunc(JobTypeDTO jobType)
    {
        return jobType != null ? Loc.TValue("jobType", jobType.Value) : "";
    }
    private string WorkLocationToStringFunc(WorkLocationTypeDTO workLocation)
    {
        return workLocation != null ? Loc.TValue("workLocation", workLocation.Value) : "";
    }


    protected async Task<IEnumerable<CompanyDTO>> SearchCompaniesFunc(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ViewModel.Companies;
        }
        return ViewModel.Companies.Where(x => x.CompanyName.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

    private void NewValue(string newValue)
    {
        var a = "b";
    }

    protected async Task<IEnumerable<JobApplicationSourceDTO>> SearchJobApplicationSourceFunc(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ViewModel.JobApplicationSources;
        }
        return ViewModel.JobApplicationSources.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

    protected async Task<IEnumerable<JobTypeDTO>> SearchJobTypesFunc(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ViewModel.JobTypes;
        }
        return ViewModel.JobTypes.Where(x => x.Value.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }

    protected async Task<IEnumerable<WorkLocationTypeDTO>> SearchWorkLocationTypesFunc(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ViewModel.WorkLocationTypes;
        }
        return ViewModel.WorkLocationTypes.Where(x => x.Value.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }


    private async Task OpenCreateNewCompanyDialog()
    {
        // Close the autocomplete dropdown first — otherwise its popover (higher z-index) covers the dialog.
        if (mudCompanyAutocomplete is not null)
        {
            await mudCompanyAutocomplete.BlurAsync();
        }

        ViewModel.CreateNewCompanyDialogVisible = true;
    }

    private string CompanyInitial
    {
        get
        {
            var name = ViewModel.JobApplication?.Company?.CompanyName;
            return string.IsNullOrWhiteSpace(name) ? "?" : name.Trim()[..1].ToUpperInvariant();
        }
    }

    private string HeaderTitle
    {
        get
        {
            var ja = ViewModel.JobApplication;
            if (ja is null || ja.Id == Guid.Empty)
            {
                return Loc.T("detail.newApplication");
            }
            return string.IsNullOrWhiteSpace(ja.JobPositionTitle) ? Loc.T("detail.jobApplication") : ja.JobPositionTitle;
        }
    }

    private string HeaderSubtitle
    {
        get
        {
            var ja = ViewModel.JobApplication;
            if (ja is null || ja.Id == Guid.Empty)
            {
                return Loc.T("detail.fillDetailsSave");
            }

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(ja.Company?.CompanyName)) parts.Add(ja.Company.CompanyName);
            if (!string.IsNullOrWhiteSpace(ja.ApplicationSource?.Name)) parts.Add(ja.ApplicationSource.Name);
            if (!string.IsNullOrWhiteSpace(ja.WorkLocationType?.Value)) parts.Add(Loc.TValue("workLocation", ja.WorkLocationType.Value));
            return string.Join(" · ", parts);
        }
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

    private static readonly string[] AllStatuses = { "Applied", "InProcess", "Offer", "Accepted", "Rejected", "Withdrawed" };

    private async Task ChangeStatus(string? newStatus)
    {
        if (string.IsNullOrEmpty(newStatus))
        {
            return;
        }

        var ok = await ViewModel.ChangeStatusAsync(newStatus);

        if (ok)
        {
            Snackbar.Add($"Status: {Loc.T(StatusKey(newStatus))}", Severity.Success);
        }
        else
        {
            Snackbar.Add("Promjena statusa nije uspjela.", Severity.Error);
        }

        StateHasChanged();
    }

    private void OpenAddStep() => ViewModel.OpenAddStep();

    private void OpenEditStep(InterviewStepDTO step) => ViewModel.OpenEditStep(step);

    private async Task SaveStep() => await ViewModel.SaveStepAsync();

    private async Task DeleteInterviewStep(Guid interviewStepId)
    {
        await ViewModel.DeleteInterviewStepAsync(interviewStepId);
    }

    private static Color OutcomeColor(string outcome) => outcome switch
    {
        "Passed" => Color.Success,
        "Failed" => Color.Error,
        _ => Color.Warning
    };
}