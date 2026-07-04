using FluentResults;
using Procoding.ApplicationTracker.DTOs.Request.Candidates;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.DTOs.Response.Candidates;
using Procoding.ApplicationTracker.DTOs.Response.JobApplications;
using Procoding.ApplicationTracker.Web.Auth;
using Procoding.ApplicationTracker.Web.Extensions;
using Procoding.ApplicationTracker.Web.Services.Interfaces;
using System.Net.Http.Headers;

namespace Procoding.ApplicationTracker.Web.Services;

public class JobApplicationService : IJobApplicationService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenProvider _tokenProvider;

    public JobApplicationService(HttpClient httpClient, ITokenProvider tokenProvider)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<JobApplicationResponseDTO>> GetJobApplicationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.GetAsync(UrlConstants.JobApplications.GetOne(id));

        return await response.HandleResponseAsync<JobApplicationResponseDTO>(cancellationToken);
    }

    public async Task<Result<JobApplicationListResponseDTO>> GetJobApplicationsAsync(JobApplicationGetListRequestDTO request, CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.GetAsync($"{UrlConstants.JobApplications.GET_ALL_URL}?{request.ToQueryString()}");

        return await response.HandleResponseAsync<JobApplicationListResponseDTO>(cancellationToken);
    }

    public async Task<Result<JobApplicationInsertedResponseDTO>> InsertJobApplicationAsync(JobApplicationInsertRequestDTO request,
                                                                                           CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.PostAsJsonAsync(UrlConstants.JobApplications.InsertUrl(), request, cancellationToken);

        return await response.HandleResponseAsync<JobApplicationInsertedResponseDTO>(cancellationToken);
    }

    public async Task<Result<ExtractedJobPostingResponseDTO>> ExtractJobPostingAsync(ExtractJobPostingRequestDTO request,
                                                                                     CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.PostAsJsonAsync(UrlConstants.JobApplications.ExtractFromText(), request, cancellationToken);

        return await response.HandleResponseAsync<ExtractedJobPostingResponseDTO>(cancellationToken);
    }

    public async Task<Result<JobApplicationUpdatedResponseDTO>> UpdateJobApplicationAsync(JobApplicationUpdateRequestDTO request,
                                                                                          CancellationToken cancellationToken = default)
    {

        await Authorize();

        var response = await _httpClient.PutAsJsonAsync(UrlConstants.JobApplications.UpdateUrl(), request, cancellationToken);

        return await response.HandleResponseAsync<JobApplicationUpdatedResponseDTO>(cancellationToken);
    }

    public async Task<Result<JobApplicationStatusChangedResponseDTO>> ChangeJobApplicationStatusAsync(JobApplicationChangeStatusRequestDTO request,
                                                                                                     CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.PatchAsJsonAsync(UrlConstants.JobApplications.ChangeStatusUrl(), request, cancellationToken);

        return await response.HandleResponseAsync<JobApplicationStatusChangedResponseDTO>(cancellationToken);
    }

    public async Task<Result<InterviewStepAddedResponseDTO>> AddInterviewStepAsync(AddInterviewStepRequestDTO request,
                                                                                   CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.PostAsJsonAsync(UrlConstants.JobApplications.InterviewStepsUrl(), request, cancellationToken);

        return await response.HandleResponseAsync<InterviewStepAddedResponseDTO>(cancellationToken);
    }

    public async Task<Result<Guid>> DeleteInterviewStepAsync(Guid jobApplicationId, Guid interviewStepId, CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.DeleteAsync(UrlConstants.JobApplications.DeleteInterviewStepUrl(jobApplicationId, interviewStepId), cancellationToken);

        return await response.HandleResponseAsync<Guid>(cancellationToken);
    }

    public async Task<Result<DTOs.Model.InterviewStepDTO>> UpdateInterviewStepAsync(UpdateInterviewStepRequestDTO request, CancellationToken cancellationToken = default)
    {
        await Authorize();

        var response = await _httpClient.PutAsJsonAsync(UrlConstants.JobApplications.InterviewStepsUrl(), request, cancellationToken);

        return await response.HandleResponseAsync<DTOs.Model.InterviewStepDTO>(cancellationToken);
    }

    private async Task Authorize()
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetAccessTokenAsync());
    }
}
