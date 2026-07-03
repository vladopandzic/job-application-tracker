using Microsoft.Extensions.DependencyInjection;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Auth;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.Infrastructure.Authentication;
using Procoding.ApplicationTracker.Infrastructure.Data;
using Procoding.ApplicationTracker.Infrastructure.Repositories;

namespace Procoding.ApplicationTracker.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistance(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(x => TimeProvider.System);

        services.AddScoped<IIdentityContext, IdentityContext>();

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IJobApplicationSourceRepository, JobApplicationSourceRepository>();

        services.AddScoped<ICompanyRepository, CompanyRepository>();

        services.AddScoped<ICandidateRepository, CandidateRepository>();

        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

        services.AddScoped<IEmployeeRepository, EmployeesRepository>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IJobTypeRepository, JobTypeRepository>();

        services.AddScoped<IWorkLocationTypeRepository, WorkLocationTypeRepository>();

        services.AddScoped<ITranslationRepository, TranslationRepository>();

        return services;
    }
}
