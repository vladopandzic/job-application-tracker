using Procoding.ApplicationTracker.DTOs.Model;
using Procoding.ApplicationTracker.DTOs.Request.Companies;
using Procoding.ApplicationTracker.DTOs.Request.JobApplications;
using Procoding.ApplicationTracker.Web.Services.Interfaces;
using Procoding.ApplicationTracker.Web.Validators;
using Procoding.ApplicationTracker.Web.ViewModels.Abstractions;

namespace Procoding.ApplicationTracker.Web.ViewModels.JobApplications;

public class MyJobApplicationViewModel : EditViewModelBase
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly IJobApplicationSourceService _jobApplicationSourceService;
    private readonly ICandidateService _candidateService;
    private readonly INotificationService _notificationService;
    private readonly IJobTypeService _jobTypeService;
    private readonly IWorkLocationTypeService _workLocationTypeService;
    private readonly ICompanyService _companyService;

    public JobApplicationDTO? JobApplication { get; set; }

    public List<JobApplicationSourceDTO> JobApplicationSources { get; set; } = new List<JobApplicationSourceDTO>();

    public List<CompanyDTO> Companies { get; set; } = new List<CompanyDTO>();

    public List<JobTypeDTO> JobTypes { get; set; } = new List<JobTypeDTO>();

    public List<WorkLocationTypeDTO> WorkLocationTypes { get; set; } = new List<WorkLocationTypeDTO>();

    //TODO: new valdiator
    public MyNewJobApplicationValidator Validator { get; }

    public CompanyValidator CompanyValidator { get; }

    public bool CreateNewCompanyDialogVisible { get; set; }

    /// <summary>Controls the "edit application details" dialog (the form).</summary>
    public bool EditDetailsDialogVisible { get; set; }

    /// <summary>Set when a save was paused to collect a new company's details — after the company is
    /// created we resume saving the application automatically.</summary>
    public bool ContinueSaveAfterCompany { get; set; }

    /// <summary>What the user pasted for AI import — either the job posting text or a single link.</summary>
    public string AiImportText { get; set; } = string.Empty;

    /// <summary>Job posting URL (set internally when the pasted content is detected to be a link).</summary>
    public string AiImportUrl { get; set; } = string.Empty;

    /// <summary>True while the AI extraction request is in flight.</summary>
    public bool IsImporting { get; set; }

    public CompanyDTO NewCompany { get; set; } = default!;

    public string PageTitle { get; set; }

    /// <summary>All selectable interview step types.</summary>
    public static readonly string[] StepTypes =
    {
        "Applied", "PhoneScreen", "TechnicalInterview", "TakeHomeTask", "SystemDesign", "Behavioral", "Final", "Offer", "Other"
    };

    /// <summary>All selectable interview step outcomes.</summary>
    public static readonly string[] Outcomes = { "Pending", "Passed", "Failed" };

    // Interview step add/edit dialog state.
    public bool StepDialogVisible { get; set; }

    public Guid? EditingStepId { get; set; }

    public string StepType { get; set; } = "PhoneScreen";

    public DateTime? StepDate { get; set; } = DateTime.Today;

    public string StepOutcome { get; set; } = "Pending";

    public string? StepNotes { get; set; }

    public string StepDialogTitle => EditingStepId is null ? "Add step" : "Edit step";

    public MyJobApplicationViewModel(IJobApplicationService jobApplicationService,
                                     ICompanyService companyService,
                                     IJobApplicationSourceService jobApplicationSourceService,
                                     ICandidateService candidateService,
                                     INotificationService notificationService,
                                     IJobTypeService jobTypeService,
                                     IWorkLocationTypeService workLocationTypeService,
                                     MyNewJobApplicationValidator validator,
                                     CompanyValidator companyValidator)
    {
        _companyService = companyService;
        _jobApplicationService = jobApplicationService;
        _jobApplicationSourceService = jobApplicationSourceService;
        _candidateService = candidateService;
        _notificationService = notificationService;
        _jobTypeService = jobTypeService;
        _workLocationTypeService = workLocationTypeService;
        Validator = validator;
        CompanyValidator = companyValidator;
    }

    public async Task InitializeViewModel(Guid? id, CancellationToken cancellationToken = default)
    {
        NewCompany = new CompanyDTO(Guid.Empty, "", "");

        await Task.WhenAll(GetCompanies(cancellationToken),
                           GetJobApplicationSources(cancellationToken),
                           GetJobTypes(cancellationToken),
                           GerWorkLocationTypes(cancellationToken));

        if (id is null)
        {
            var candidate = new CandidateDTO(Guid.Empty, "", "", "");
            var jobApplicationSource = new JobApplicationSourceDTO(Guid.Empty, "");
            var company = new CompanyDTO(Guid.Empty, "", "");

            JobApplication = new JobApplicationDTO(id: Guid.Empty,
                                                   candidate: candidate,
                                                   applicationSource: jobApplicationSource,
                                                   company: company,
                                                   jobPositionTitle: "",
                                                   jobAdLink: "",
                                                   jobType: new JobTypeDTO(""),
                                                   workLocationType: new WorkLocationTypeDTO(""),
                                                   description: null);
            EditDetailsDialogVisible = true;
            SetPageTitle();

            return;
        }
        IsLoading = true;
        var response = await _jobApplicationService.GetJobApplicationAsync(id.Value);
        IsLoading = false;

        if (response is not null)
        {
            JobApplication = response.Value.JobApplication;
        }

        SetPageTitle();
    }

    private void SetPageTitle()
    {
        PageTitle = JobApplication?.Id == Guid.Empty ? "New job application" : $"Job application for {JobApplication?.Company?.CompanyName}";
    }

    /// <summary>
    /// Resolves the chosen company before saving: reuses an existing one by name, or creates a new one
    /// on the fly from the typed/AI-filled name + website — so the user never needs the separate
    /// "create company" dialog. Falls back to that dialog only when a new company has no website
    /// (the domain requires a valid URL). Returns false when the save should pause for that input.
    /// </summary>
    private async Task<bool> EnsureCompanyAsync()
    {
        var company = JobApplication?.Company;

        if (company is null || string.IsNullOrWhiteSpace(company.CompanyName) || company.Id != Guid.Empty)
        {
            return true; // nothing typed, or already a persisted company
        }

        var existing = Companies.FirstOrDefault(c => string.Equals(c.CompanyName, company.CompanyName, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            JobApplication!.Company = existing;
            return true;
        }

        var website = Validators.FluentValidationExtensions.NormalizeUrl(company.OfficialWebSiteLink);
        if (string.IsNullOrWhiteSpace(website))
        {
            // New company but no website to satisfy the domain — collect it in the dialog, then resume the save.
            NewCompany = new CompanyDTO(Guid.Empty, company.CompanyName, string.Empty);
            ContinueSaveAfterCompany = true;
            CreateNewCompanyDialogVisible = true;
            return false;
        }

        var result = await _companyService.InsertCompanyAsync(new CompanyInsertRequestDTO(company.CompanyName, website));
        if (result.IsSuccess)
        {
            Companies.Add(result.Value.Company);
            JobApplication!.Company = result.Value.Company;
            return true;
        }

        // Backend rejected it (e.g. invalid URL) — fall back to the dialog so the user can fix it, then resume.
        NewCompany = new CompanyDTO(Guid.Empty, company.CompanyName, company.OfficialWebSiteLink);
        ContinueSaveAfterCompany = true;
        CreateNewCompanyDialogVisible = true;
        _notificationService.ShowMessageFromResult(result);
        return false;
    }

    public async Task SaveAsync()
    {
        IsSaving = true;

        if (!await EnsureCompanyAsync())
        {
            IsSaving = false;
            return; // paused: company needs completing in the dialog first
        }

        var isJobApplicationValid = (await Validator.ValidateAsync(JobApplication!)).IsValid;

        if (!isJobApplicationValid)
        {
            IsSaving = false;
            return;
        }

        // Prepend https:// when the user typed a scheme-less link so the stored URL is clickable.
        JobApplication!.JobAdLink = Validators.FluentValidationExtensions.NormalizeUrl(JobApplication.JobAdLink);

        if (JobApplication!.Id == Guid.Empty)
        {
            var result = await _jobApplicationService
                    .InsertJobApplicationAsync(new JobApplicationInsertRequestDTO(JobApplicationSourceId: JobApplication.ApplicationSource!.Id,
                                                                                  CompanyId: JobApplication.Company!.Id,
                                                                                  JobPositionTitle: JobApplication.JobPositionTitle,
                                                                                  JobAdLink: JobApplication.JobAdLink,
                                                                                  JobType: JobApplication.JobType,
                                                                                  WorkLocationType: JobApplication.WorkLocationType,
                                                                                  Description: JobApplication.Description));

            if (result.IsSuccess)
            {
                JobApplication.Id = result.Value.JobApplication.Id;
                EditDetailsDialogVisible = false;
            }
            _notificationService.ShowMessageFromResult(result);
        }
        else
        {
            var result =
                await _jobApplicationService.UpdateJobApplicationAsync(new JobApplicationUpdateRequestDTO(Id: JobApplication.Id,
                                                                                                          JobApplicationSourceId: JobApplication.ApplicationSource!.Id,
                                                                                                          CompanyId: JobApplication.Company!.Id,
                                                                                                          JobPositionTitle: JobApplication.JobPositionTitle,
                                                                                                          JobAdLink: JobApplication.JobAdLink,
                                                                                                          JobType: JobApplication.JobType,
                                                                                                          WorkLocationType: JobApplication.WorkLocationType,
                                                                                                          Description: JobApplication.Description));

            if (result.IsSuccess)
            {
                EditDetailsDialogVisible = false;
            }
            _notificationService.ShowMessageFromResult(result);
        }

        IsSaving = false;
    }

    /// <summary>Changes the application's status from the detail page (same command the board uses).</summary>
    public async Task<bool> ChangeStatusAsync(string newStatus, CancellationToken cancellationToken = default)
    {
        if (JobApplication is null || JobApplication.Id == Guid.Empty || string.IsNullOrEmpty(newStatus) || newStatus == JobApplication.Status)
        {
            return false;
        }

        IsSaving = true;
        var result = await _jobApplicationService.ChangeJobApplicationStatusAsync(
            new JobApplicationChangeStatusRequestDTO(JobApplication.Id, newStatus), cancellationToken);
        IsSaving = false;

        if (result.IsSuccess)
        {
            JobApplication.Status = newStatus;
        }

        return result.IsSuccess;
    }

    /// <summary>Archives the current application. Returns true on success.</summary>
    public async Task<bool> ArchiveAsync(CancellationToken cancellationToken = default)
    {
        if (JobApplication is null || JobApplication.Id == Guid.Empty)
        {
            return false;
        }

        IsSaving = true;
        var result = await _jobApplicationService.ArchiveJobApplicationAsync(JobApplication.Id, cancellationToken);
        IsSaving = false;

        if (result.IsFailed)
        {
            _notificationService.ShowMessageFromResult(result);
        }

        return result.IsSuccess;
    }

    public async Task SaveNewCompany()
    {
        var isCompanyValid = (await CompanyValidator.ValidateAsync(NewCompany!)).IsValid;
        if (!isCompanyValid)
        {
            return;
        }

        IsSaving = true;

        NewCompany!.OfficialWebSiteLink = Validators.FluentValidationExtensions.NormalizeUrl(NewCompany.OfficialWebSiteLink);

        if (NewCompany!.Id == Guid.Empty)
        {
            var result = await _companyService.InsertCompanyAsync(new CompanyInsertRequestDTO(NewCompany!.CompanyName, NewCompany.OfficialWebSiteLink));

            if (result.IsSuccess)
            {
                Companies.Add(result.Value.Company);
                JobApplication!.Company = result.Value.Company;
                CreateNewCompanyDialogVisible = false;
                NewCompany = new CompanyDTO(Guid.Empty, "", "");

                // If we opened this dialog mid-save, finish saving the application now that the company exists.
                if (ContinueSaveAfterCompany)
                {
                    ContinueSaveAfterCompany = false;
                    IsSaving = false;
                    await SaveAsync();
                    return;
                }
            }
            _notificationService.ShowMessageFromResult(result);
        }


        IsSaving = false;
    }

    /// <summary>
    /// Sends the pasted posting text to the AI extractor and pre-fills the form with what it returns.
    /// Only overwrites fields the model actually produced; the user reviews before saving.
    /// Returns a short status: null on success, otherwise an error message to surface.
    /// </summary>
    /// <summary>
    /// Single entry point for AI import — auto-detects whether the pasted content is a link or the raw
    /// posting text, so the user doesn't have to pick a mode. Returns null on success, else an error.
    /// </summary>
    public async Task<string?> ImportAsync(CancellationToken cancellationToken = default)
    {
        var content = (AiImportText ?? string.Empty).Trim();

        if (content.Length == 0)
        {
            return null;
        }

        if (LooksLikeUrl(content))
        {
            AiImportUrl = content;
            return await ImportFromUrlAsync(cancellationToken);
        }

        if (content.Length < 30)
        {
            return "Zalijepi cijeli tekst oglasa ili link na oglas.";
        }

        return await ImportFromTextAsync(cancellationToken);
    }

    /// <summary>A single token starting with http(s):// and no whitespace is treated as a link, not text.</summary>
    private static bool LooksLikeUrl(string content)
    {
        return !content.Any(char.IsWhiteSpace)
               && Uri.TryCreate(content, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public async Task<string?> ImportFromTextAsync(CancellationToken cancellationToken = default)
    {
        if (JobApplication is null || string.IsNullOrWhiteSpace(AiImportText) || AiImportText.Trim().Length < 30)
        {
            return null;
        }

        IsImporting = true;
        var result = await _jobApplicationService.ExtractJobPostingAsync(
            new DTOs.Request.JobApplications.ExtractJobPostingRequestDTO { Content = AiImportText }, cancellationToken);
        IsImporting = false;

        if (result.IsFailed)
        {
            return result.Errors.FirstOrDefault()?.Message ?? "AI import nije uspio.";
        }

        ApplyExtractedToForm(result.Value);
        return null;
    }

    /// <summary>
    /// Fetches the posting from a URL via the AI extractor and pre-fills the form. Some sites block bots,
    /// so this may fail — the error message tells the user to paste text instead.
    /// </summary>
    public async Task<string?> ImportFromUrlAsync(CancellationToken cancellationToken = default)
    {
        if (JobApplication is null || string.IsNullOrWhiteSpace(AiImportUrl))
        {
            return null;
        }

        IsImporting = true;
        var result = await _jobApplicationService.ExtractJobPostingFromUrlAsync(
            new DTOs.Request.JobApplications.ExtractJobPostingFromUrlRequestDTO { Url = AiImportUrl.Trim() }, cancellationToken);
        IsImporting = false;

        if (result.IsFailed)
        {
            return result.Errors.FirstOrDefault()?.Message ?? "AI import nije uspio.";
        }

        ApplyExtractedToForm(result.Value);
        return null;
    }

    /// <summary>Pre-fills the form from an AI extraction result — only fields the model actually produced.</summary>
    private void ApplyExtractedToForm(DTOs.Response.JobApplications.ExtractedJobPostingResponseDTO extracted)
    {
        if (JobApplication is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(extracted.PositionTitle))
        {
            JobApplication.JobPositionTitle = extracted.PositionTitle;
        }

        if (!string.IsNullOrWhiteSpace(extracted.Description))
        {
            JobApplication.Description = extracted.Description.Length > 500 ? extracted.Description[..500] : extracted.Description;
        }

        if (!string.IsNullOrWhiteSpace(extracted.JobAdLink))
        {
            JobApplication.JobAdLink = extracted.JobAdLink;
        }

        if (!string.IsNullOrWhiteSpace(extracted.JobType))
        {
            var match = JobTypes.FirstOrDefault(t => string.Equals(t.Value, extracted.JobType, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                JobApplication.JobType = match;
            }
        }

        if (!string.IsNullOrWhiteSpace(extracted.WorkLocationType))
        {
            var match = WorkLocationTypes.FirstOrDefault(w => string.Equals(w.Value, extracted.WorkLocationType, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                JobApplication.WorkLocationType = match;
            }
        }

        if (!string.IsNullOrWhiteSpace(extracted.CompanyName))
        {
            var existing = Companies.FirstOrDefault(c => string.Equals(c.CompanyName, extracted.CompanyName, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                JobApplication.Company = existing;
            }
            else
            {
                // Show the name in the field right away; the company is created automatically on save
                // (EnsureCompanyAsync) using the extracted website — no separate dialog needed.
                JobApplication.Company = new CompanyDTO(Guid.Empty, extracted.CompanyName, extracted.CompanyWebsite ?? string.Empty);
                NewCompany = new CompanyDTO(Guid.Empty, extracted.CompanyName, extracted.CompanyWebsite ?? string.Empty);
            }
        }
    }

    private async Task GetCompanies(CancellationToken cancellationToken)
    {
        var companiesResult = await _companyService.GetCompaniesAsync(cancellationToken);
        if (companiesResult.IsSuccess)
        {
            Companies = companiesResult.Value.Companies.ToList();
        }
    }

    private async Task GetJobTypes(CancellationToken cancellationToken)
    {
        var jobTypesResult = await _jobTypeService.GetJobTypesAsync(cancellationToken);
        if (jobTypesResult.IsSuccess)
        {
            JobTypes = jobTypesResult.Value.JobTypes.ToList();
        }
    }

    private async Task GerWorkLocationTypes(CancellationToken cancellationToken)
    {
        var workLocationTypesResult = await _workLocationTypeService.GetWorkLocationTypesAsync(cancellationToken);
        if (workLocationTypesResult.IsSuccess)
        {
            WorkLocationTypes = workLocationTypesResult.Value.WorkLocationTypes.ToList();
        }
    }

    private async Task GetJobApplicationSources(CancellationToken cancellationToken)
    {
        var jobApplicationSourceResult = await _jobApplicationSourceService.GetJobApplicationSourcesAsync(cancellationToken);
        if (jobApplicationSourceResult.IsSuccess)
        {
            JobApplicationSources = jobApplicationSourceResult.Value.JobApplicationSources.ToList();
        }
    }

    public void OpenCreateNewCompanyDialog()
    {
        NewCompany = new CompanyDTO(Guid.Empty, "", "");

        CreateNewCompanyDialogVisible = true;
    }

    /// <summary>
    /// Opens the dialog to add a new interview step.
    /// </summary>
    public void OpenAddStep()
    {
        EditingStepId = null;
        StepType = "PhoneScreen";
        StepDate = DateTime.Today;
        StepOutcome = "Pending";
        StepNotes = null;
        StepDialogVisible = true;
    }

    /// <summary>
    /// Opens the dialog to edit an existing interview step.
    /// </summary>
    public void OpenEditStep(InterviewStepDTO step)
    {
        EditingStepId = step.Id;
        StepType = step.Type;
        StepDate = step.OccurredOn;
        StepOutcome = step.Outcome;
        StepNotes = step.Notes;
        StepDialogVisible = true;
    }

    /// <summary>
    /// Saves the interview step dialog — adds a new step or updates the one being edited.
    /// </summary>
    public async Task SaveStepAsync(CancellationToken cancellationToken = default)
    {
        if (JobApplication is null || JobApplication.Id == Guid.Empty)
        {
            return;
        }

        IsSaving = true;

        if (EditingStepId is null)
        {
            var result = await _jobApplicationService.AddInterviewStepAsync(
                new AddInterviewStepRequestDTO(JobApplication.Id, StepType, StepDate ?? DateTime.Today, StepOutcome, StepNotes),
                cancellationToken);

            if (result.IsSuccess)
            {
                JobApplication.InterviewSteps.Add(result.Value.InterviewStep);
                JobApplication.InterviewSteps = JobApplication.InterviewSteps.OrderBy(s => s.OccurredOn).ToList();
                StepDialogVisible = false;
            }

            _notificationService.ShowMessageFromResult(result);
        }
        else
        {
            var result = await _jobApplicationService.UpdateInterviewStepAsync(
                new UpdateInterviewStepRequestDTO(JobApplication.Id, EditingStepId.Value, StepType, StepDate ?? DateTime.Today, StepOutcome, StepNotes),
                cancellationToken);

            if (result.IsSuccess)
            {
                var index = JobApplication.InterviewSteps.FindIndex(s => s.Id == EditingStepId.Value);
                if (index >= 0)
                {
                    JobApplication.InterviewSteps[index] = result.Value;
                }
                JobApplication.InterviewSteps = JobApplication.InterviewSteps.OrderBy(s => s.OccurredOn).ToList();
                StepDialogVisible = false;
            }

            _notificationService.ShowMessageFromResult(result);
        }

        IsSaving = false;
    }

    /// <summary>
    /// Deletes an interview step from the current job application.
    /// </summary>
    public async Task DeleteInterviewStepAsync(Guid interviewStepId, CancellationToken cancellationToken = default)
    {
        if (JobApplication is null || JobApplication.Id == Guid.Empty)
        {
            return;
        }

        IsSaving = true;

        var result = await _jobApplicationService.DeleteInterviewStepAsync(JobApplication.Id, interviewStepId, cancellationToken);

        if (result.IsSuccess)
        {
            JobApplication.InterviewSteps = JobApplication.InterviewSteps.Where(s => s.Id != interviewStepId).ToList();
        }

        _notificationService.ShowMessageFromResult(result);

        IsSaving = false;
    }
}
