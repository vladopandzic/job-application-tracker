using FluentResults;
using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;
using Procoding.ApplicationTracker.Web.Services.Interfaces;

namespace Procoding.ApplicationTracker.Web.ViewModels.JobApplications;

public class JobApplicationsKanbanViewModel : ViewModelBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public JobApplicationsKanbanViewModel(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    /// <summary>
    /// All job applications shown on the board. Mutable so the drop container can move cards between columns.
    /// </summary>
    public List<JobApplicationDTO> JobApplications { get; set; } = new();

    public async Task InitializeViewModel(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        // The board shows every application at once, so ask for a single large page.
        var request = new JobApplicationGetListRequestDTO { PageNumber = 1, PageSize = 1000 };
        var response = await _jobApplicationService.GetJobApplicationsAsync(request, cancellationToken);

        IsLoading = false;

        if (response.IsSuccess)
        {
            JobApplications = response.Value.JobApplications.ToList();
        }
    }

    public async Task<Result<JobApplicationStatusChangedResponseDTO>> ChangeStatusAsync(Guid jobApplicationId,
                                                                                        string newStatus,
                                                                                        CancellationToken cancellationToken = default)
    {
        return await _jobApplicationService.ChangeJobApplicationStatusAsync(
            new JobApplicationChangeStatusRequestDTO(jobApplicationId, newStatus), cancellationToken);
    }
}
