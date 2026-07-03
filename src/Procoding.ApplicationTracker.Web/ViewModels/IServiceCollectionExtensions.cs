using Procoding.ApplicationTracker.Web.ViewModels.Auth;
using Procoding.ApplicationTracker.Web.ViewModels.Candidates;
using Procoding.ApplicationTracker.Web.ViewModels.Companies;
using Procoding.ApplicationTracker.Web.ViewModels.Employees;
using Procoding.ApplicationTracker.Web.ViewModels.JobApplications;
using Procoding.ApplicationTracker.Web.ViewModels.JobApplicationSources;

namespace Procoding.ApplicationTracker.Web.ViewModels;

public static class IServiceCollectionExtensions
{
    public static void AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<JobApplicationSourceListViewModel, JobApplicationSourceListViewModel>();
        services.AddTransient<JobApplicationSourceDetailsViewModel, JobApplicationSourceDetailsViewModel>();

        services.AddTransient<CandidateListViewModel, CandidateListViewModel>();
        services.AddTransient<CandidateDetailsViewModel, CandidateDetailsViewModel>();

        services.AddTransient<CompanyListViewModel, CompanyListViewModel>();
        services.AddTransient<CompanyDetailsViewModel, CompanyDetailsViewModel>();

        services.AddTransient<JobApplicationListViewModel, JobApplicationListViewModel>();
        services.AddTransient<JobApplicationDetailsViewModel, JobApplicationDetailsViewModel>();

        services.AddTransient<EmployeeListViewModel, EmployeeListViewModel>();
        services.AddTransient<EmployeeDetailsViewModel, EmployeeDetailsViewModel>();

        services.AddTransient<LoginViewModel, LoginViewModel>();

        services.AddTransient<MyJobApplicationViewModel, MyJobApplicationViewModel>();

        services.AddTransient<MyJobApplicationsListViewModel, MyJobApplicationsListViewModel>();

        services.AddTransient<JobApplicationsKanbanViewModel, JobApplicationsKanbanViewModel>();

        services.AddTransient<DashboardViewModel, DashboardViewModel>();

        services.AddTransient<Translations.TranslationsViewModel, Translations.TranslationsViewModel>();

    }
}
