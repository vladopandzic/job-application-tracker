using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Procoding.ApplicationTracker.Web.Localization;
using Procoding.ApplicationTracker.Web.ViewModels;

namespace Procoding.ApplicationTracker.Web.Pages;

public partial class Home : IDisposable
{
    [Inject]
    public DashboardViewModel ViewModel { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Inject]
    public LocalizationService Loc { get; set; } = default!;

    private string _firstName = "";

    private static readonly (string Status, string Label, string Color)[] Funnel =
    {
        ("Applied", "Applied", "#2563EB"),
        ("InProcess", "In process", "#F59E0B"),
        ("Offer", "Offer", "#7C3AED"),
        ("Accepted", "Accepted", "#16A34A"),
        ("Rejected", "Rejected", "#DC2626"),
        ("Withdrawed", "Withdrawn", "#6B7280"),
    };

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        _firstName = authState.User.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value
                     ?? authState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                     ?? "";

        await Loc.EnsureLoadedAsync();
        Loc.OnChange += OnLocalizationChanged;

        await ViewModel.InitializeViewModel();
        await base.OnInitializedAsync();
    }

    private void OnLocalizationChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Loc.OnChange -= OnLocalizationChanged;

    private static string StepTypeLabel(string type) => type switch
    {
        "PhoneScreen" => "Phone / HR screen",
        "TechnicalInterview" => "Technical interview",
        "TakeHomeTask" => "Take-home task",
        "SystemDesign" => "System design",
        _ => type
    };

    private static Color OutcomeColor(string outcome) => outcome switch
    {
        "Passed" => Color.Success,
        "Failed" => Color.Error,
        _ => Color.Warning
    };
}
