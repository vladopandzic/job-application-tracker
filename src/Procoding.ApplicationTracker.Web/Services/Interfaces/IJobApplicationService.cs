using FluentResults;
using Procoding.ApplicationTracker.DTOs.Request.Candidates;
using Procoding.ApplicationTracker.DTOs.Request.Companies;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.Candidates;
using Procoding.ApplicationTracker.DTOs.Response.Companies;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;

namespace Procoding.ApplicationTracker.Web.Services.Interfaces;

public interface IJobApplicationService
{
    /// <summary>
    /// Gets jobApplication by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Result<JobApplicationResponseDTO>> GetJobApplicationAsync(Guid id, CancellationToken cancellationToken = default);


    /// <summary>
    /// Gets all job applications.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<JobApplicationListResponseDTO>> GetJobApplicationsAsync(JobApplicationGetListRequestDTO request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts one jobApplication.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Result<JobApplicationInsertedResponseDTO>> InsertJobApplicationAsync(JobApplicationInsertRequestDTO request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates one jobApplication.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Result<JobApplicationUpdatedResponseDTO>> UpdateJobApplicationAsync(JobApplicationUpdateRequestDTO request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the status of one job application.
    /// </summary>
    /// <param name="request">Request containing the job application id and the target status.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<JobApplicationStatusChangedResponseDTO>> ChangeJobApplicationStatusAsync(JobApplicationChangeStatusRequestDTO request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an interview step to a job application.
    /// </summary>
    Task<Result<InterviewStepAddedResponseDTO>> AddInterviewStepAsync(AddInterviewStepRequestDTO request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an interview step from a job application.
    /// </summary>
    Task<Result<Guid>> DeleteInterviewStepAsync(Guid jobApplicationId, Guid interviewStepId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing interview step.
    /// </summary>
    Task<Result<Procoding.ApplicationTracker.DTOs.Model.InterviewStepDTO>> UpdateInterviewStepAsync(UpdateInterviewStepRequestDTO request, CancellationToken cancellationToken = default);
}
