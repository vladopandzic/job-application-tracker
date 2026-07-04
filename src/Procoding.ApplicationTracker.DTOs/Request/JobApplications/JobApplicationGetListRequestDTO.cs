using Procoding.ApplicationTracker.DTOs.Request.Base;

namespace Procoding.ApplicationTracker.DTOs.Request.JobApplications;

public class JobApplicationGetListRequestDTO : BaseListingRequestDTO
{
    /// <summary>When true, returns only archived applications; otherwise only active (non-archived) ones.</summary>
    public bool Archived { get; set; }
}
