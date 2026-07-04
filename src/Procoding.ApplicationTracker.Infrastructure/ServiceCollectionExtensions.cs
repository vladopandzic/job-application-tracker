using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Emailing;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Auth;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.Infrastructure.Authentication;
using Procoding.ApplicationTracker.Infrastructure.Data;
using Procoding.ApplicationTracker.Infrastructure.Emailing;
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

    /// <summary>
    /// Registers transactional email sending (SMTP). Options are bound from the "Email" configuration
    /// section (populated from secrets at deploy). Safe when unconfigured — the sender no-ops.
    /// </summary>
    public static IServiceCollection AddEmailSending(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpEmailOptions>(configuration.GetSection(SmtpEmailOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
