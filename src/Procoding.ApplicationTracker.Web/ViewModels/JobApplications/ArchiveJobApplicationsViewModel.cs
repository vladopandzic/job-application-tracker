using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.Web.Services.Interfaces;
using Procoding.ApplicationTracker.Web.ViewModels.Abstractions;

namespace Procoding.ApplicationTracker.Web.ViewModels.JobApplications;

/// <summary>
/// Lists archived applications so the candidate can restore them or delete them for good.
/// </summary>
public class ArchiveJobApplicationsViewModel : EditViewModelBase
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly INotificationService _notificationService;

    public List<JobApplicationDTO> JobApplications { get; set; } = new();

    public ArchiveJobApplicationsViewModel(IJobApplicationService jobApplicationService, INotificationService notificationService)
    {
        _jobApplicationService = jobApplicationService;
        _notificationService = notificationService;
    }

    public async Task InitializeViewModel(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        var request = new JobApplicationGetListRequestDTO { PageNumber = 1, PageSize = 1000, Archived = true };
        var response = await _jobApplicationService.GetJobApplicationsAsync(request, cancellationToken);
        IsLoading = false;

        if (response.IsSuccess)
        {
            JobApplications = response.Value.JobApplications.ToList();
        }
    }

    public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        IsSaving = true;
        var result = await _jobApplicationService.UnarchiveJobApplicationAsync(id, cancellationToken);
        IsSaving = false;

        if (result.IsSuccess)
        {
            JobApplications = JobApplications.Where(x => x.Id != id).ToList();
        }
        else
        {
            _notificationService.ShowMessageFromResult(result);
        }

        return result.IsSuccess;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        IsSaving = true;
        var result = await _jobApplicationService.DeleteJobApplicationAsync(id, cancellationToken);
        IsSaving = false;

        if (result.IsSuccess)
        {
            JobApplications = JobApplications.Where(x => x.Id != id).ToList();
        }
        else
        {
            _notificationService.ShowMessageFromResult(result);
        }

        return result.IsSuccess;
    }
}
