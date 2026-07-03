using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.ValueObjects;

namespace Procoding.ApplicationTracker.Infrastructure.Data;

/// <summary>
/// Development-only seeder. Creates a demo candidate, a demo employee and a spread of job applications
/// across every status so the Kanban board has something to show. No-op if candidates already exist.
/// </summary>
public static class DevDataSeeder
{
    public const string DemoCandidateEmail = "demo@jobflow.app";
    public const string DemoCandidatePassword = "Demo123!";
    public const string DemoEmployeeEmail = "admin@jobflow.app";
    public const string DemoEmployeePassword = "Admin123!";

    public static async Task SeedAsync(ApplicationDbContext db,
                                       UserManager<Candidate> candidateManager,
                                       UserManager<Employee> employeeManager,
                                       TimeProvider timeProvider)
    {
        if (await db.Candidates.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        var sources = new List<JobApplicationSource>
        {
            JobApplicationSource.Create(Guid.NewGuid(), "LinkedIn"),
            JobApplicationSource.Create(Guid.NewGuid(), "RemoteOK"),
            JobApplicationSource.Create(Guid.NewGuid(), "Referral"),
        };
        await db.JobApplicationSources.AddRangeAsync(sources);

        var companies = new List<Company>
        {
            Company.Create(new CompanyName("Acme Corp"), new Link("https://acme.example.com")),
            Company.Create(new CompanyName("Globex"), new Link("https://globex.example.com")),
            Company.Create(new CompanyName("Initech"), new Link("https://initech.example.com")),
            Company.Create(new CompanyName("Umbrella"), new Link("https://umbrella.example.com")),
        };
        await db.Companies.AddRangeAsync(companies);
        await db.SaveChangesAsync();

        // Demo employee (admin view).
        var employee = Employee.Create(Guid.NewGuid(), "Demo", "Admin", new Email(DemoEmployeeEmail), DemoEmployeePassword, employeeManager.PasswordHasher);
        await employeeManager.CreateAsync(employee);

        // Demo candidate (the B2C user).
        var candidate = Candidate.Create(Guid.NewGuid(), "Demo", "Candidate", new Email(DemoCandidateEmail), DemoCandidatePassword, candidateManager.PasswordHasher);
        await candidateManager.CreateAsync(candidate);

        JobApplication Make(string title, int companyIndex, int sourceIndex, WorkLocationType location, JobType type, params JobApplicationStatus[] transitions)
        {
            var app = JobApplication.Create(candidate: candidate,
                                            id: Guid.NewGuid(),
                                            jobApplicationSource: sources[sourceIndex],
                                            company: companies[companyIndex],
                                            jobPositionTitle: title,
                                            jobAdLink: new Link("https://jobs.example.com/posting"),
                                            jobType: type,
                                            workLocationType: location,
                                            description: "Demo application seeded for local development.",
                                            timeProvider: timeProvider);

            foreach (var status in transitions)
            {
                app.ChangeStatus(status);
            }

            return app;
        }

        var applications = new List<JobApplication>
        {
            Make(".NET Backend Engineer", 0, 0, WorkLocationType.Remote, JobType.FullTime),
            Make("Full-Stack Developer", 1, 0, WorkLocationType.Hybrid, JobType.FullTime),
            Make("Senior C# Engineer", 2, 1, WorkLocationType.Remote, JobType.Contract, JobApplicationStatus.InProcess),
            Make("Software Engineer II", 3, 2, WorkLocationType.OnSite, JobType.FullTime, JobApplicationStatus.InProcess),
            Make("Lead Backend Developer", 0, 1, WorkLocationType.Remote, JobType.FullTime, JobApplicationStatus.InProcess, JobApplicationStatus.Offer),
            Make("Principal Engineer", 1, 2, WorkLocationType.Hybrid, JobType.FullTime, JobApplicationStatus.InProcess, JobApplicationStatus.Offer, JobApplicationStatus.Accepted),
            Make("Junior Developer", 2, 0, WorkLocationType.OnSite, JobType.FullTime, JobApplicationStatus.Rejected),
            Make("Contract .NET Dev", 3, 1, WorkLocationType.Remote, JobType.Contract, JobApplicationStatus.Withdrawed),
        };

        await db.JobApplications.AddRangeAsync(applications);
        await db.SaveChangesAsync();
    }
}
