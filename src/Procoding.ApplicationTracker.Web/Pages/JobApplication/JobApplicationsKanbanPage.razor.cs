using System.Linq;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.Web.Localization;
using Procoding.ApplicationTracker.Web.ViewModels.JobApplications;

namespace Procoding.ApplicationTracker.Web.Pages.JobApplication;

public partial class JobApplicationsKanbanPage : IDisposable
{
    [Inject]
    public JobApplicationsKanbanViewModel ViewModel { get; set; } = default!;

    [Inject]
    public ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    public LocalizationService Loc { get; set; } = default!;

    private MudDropContainer<JobApplicationDTO>? _dropContainer;

    /// <summary>
    /// Board columns. The <see cref="KanbanColumn.Status"/> must match the
    /// <c>JobApplicationStatus</c> enum name exactly, since cards are grouped by their status string.
    /// <see cref="KanbanColumn.TranslationKey"/> is the localization key for the column label.
    /// </summary>
    private sealed record KanbanColumn(string Status, string TranslationKey, Color Color);

    private static readonly KanbanColumn[] Columns =
    [
        new("Applied", "status.applied", Color.Info),
        new("InProcess", "status.inProcess", Color.Warning),
        new("Offer", "status.offer", Color.Secondary),
        new("Accepted", "status.accepted", Color.Success),
        new("Rejected", "status.rejected", Color.Error),
        new("Withdrawed", "status.withdrawed", Color.Dark),
    ];

    private bool _refreshBoard;

    protected override async Task OnInitializedAsync()
    {
        await Loc.EnsureLoadedAsync();
        await ViewModel.InitializeViewModel();
        _refreshBoard = true;
        Loc.OnChange += OnLocalizationChanged;
        await base.OnInitializedAsync();
    }

    // MudDropContainer caches its rendered items; after the data (and translations) finish loading we
    // must Refresh() once so the cards actually get placed into their status columns.
    protected override void OnAfterRender(bool firstRender)
    {
        if (_refreshBoard)
        {
            _refreshBoard = false;
            _dropContainer?.Refresh();
        }
    }

    private void OnLocalizationChanged()
    {
        InvokeAsync(() =>
        {
            StateHasChanged();
            _dropContainer?.Refresh();
        });
    }

    public void Dispose() => Loc.OnChange -= OnLocalizationChanged;

    private async Task OnItemDropped(MudItemDropInfo<JobApplicationDTO> dropInfo)
    {
        var item = dropInfo.Item;

        if (item is null)
        {
            return;
        }

        var newStatus = dropInfo.DropzoneIdentifier;
        var oldStatus = item.Status;

        if (newStatus == oldStatus)
        {
            return;
        }

        // Move the card optimistically; the domain state machine is the source of truth, so revert if it rejects.
        item.Status = newStatus;

        var result = await ViewModel.ChangeStatusAsync(item.Id, newStatus);

        if (result.IsSuccess)
        {
            Snackbar.Add($"Moved '{item.JobPositionTitle}' to {newStatus}.", Severity.Success);
        }
        else
        {
            item.Status = oldStatus;
            Snackbar.Add(string.Join(" ", result.Errors.Select(e => e.Message)), Severity.Error);
        }

        _dropContainer?.Refresh();
    }

    /// <summary>
    /// Mobile status change: drag-and-drop doesn't work on touch, so cards expose a "Move to" menu
    /// that calls the same command the drag path uses.
    /// </summary>
    private async Task MoveTo(JobApplicationDTO item, string? newStatus)
    {
        var oldStatus = item.Status;
        if (string.IsNullOrEmpty(newStatus) || newStatus == oldStatus)
        {
            return;
        }

        item.Status = newStatus;

        var result = await ViewModel.ChangeStatusAsync(item.Id, newStatus);

        if (result.IsSuccess)
        {
            var label = Columns.FirstOrDefault(c => c.Status == newStatus)?.TranslationKey ?? newStatus;
            Snackbar.Add($"'{item.JobPositionTitle}' → {Loc.T(label)}", Severity.Success);
        }
        else
        {
            item.Status = oldStatus;
            Snackbar.Add(string.Join(" ", result.Errors.Select(e => e.Message)), Severity.Error);
        }

        StateHasChanged();
    }

    private static Color OutcomeColor(string outcome) => outcome switch
    {
        "Passed" => Color.Success,
        "Failed" => Color.Error,
        _ => Color.Warning
    };
}
