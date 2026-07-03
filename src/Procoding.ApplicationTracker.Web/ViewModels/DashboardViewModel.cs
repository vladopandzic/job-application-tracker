using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.Web.Services.Interfaces;

namespace Procoding.ApplicationTracker.Web.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public DashboardViewModel(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    public IReadOnlyCollection<JobApplicationDTO> Applications { get; private set; } = new List<JobApplicationDTO>();

    public List<ActivityItem> RecentActivity { get; private set; } = new();

    public int Total => Applications.Count;

    public int CountFor(string status) => Applications.Count(a => a.Status == status);

    public int InProcess => CountFor("InProcess");

    public int Offers => CountFor("Offer");

    public int Accepted => CountFor("Accepted");

    /// <summary>Share of applications that got any response (moved past "Applied", excluding withdrawn).</summary>
    public int ResponseRatePercent =>
        Total == 0 ? 0 : (int)Math.Round(100.0 * Applications.Count(a => a.Status is "InProcess" or "Offer" or "Accepted" or "Rejected") / Total);

    public async Task InitializeViewModel(CancellationToken cancellationToken = default)
    {
        IsLoading = true;

        var response = await _jobApplicationService.GetJobApplicationsAsync(
            new JobApplicationGetListRequestDTO { PageNumber = 1, PageSize = 1000 }, cancellationToken);

        IsLoading = false;

        if (response.IsSuccess)
        {
            Applications = response.Value.JobApplications;

            RecentActivity = Applications
                .SelectMany(a => a.InterviewSteps.Select(s => new ActivityItem(a.Company?.CompanyName ?? "", a.JobPositionTitle, s.Type, s.Outcome, s.OccurredOn)))
                .OrderByDescending(x => x.When)
                .Take(6)
                .ToList();
        }
    }

    public record ActivityItem(string Company, string Position, string Type, string Outcome, DateTime When);
}
