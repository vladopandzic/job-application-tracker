using Microsoft.AspNetCore.Identity;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.ValueObjects;

namespace Procoding.ApplicationTracker.Infrastructure.Data;

/// <summary>
/// Production-safe admin bootstrap. Creates a single employee (admin) account from credentials supplied
/// via configuration — never hardcoded — so the password lives only in secrets, not in source.
/// Idempotent: no-op when the credentials are not configured or an employee with that email already
/// exists, so it is safe to run on every startup.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db,
                                       UserManager<Employee> employeeManager,
                                       string? email,
                                       string? password,
                                       string? firstName,
                                       string? lastName)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            // No admin configured — nothing to do. Keeps startup safe when the secret isn't set.
            return;
        }

        if (await employeeManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var employee = Employee.Create(Guid.NewGuid(),
                                       string.IsNullOrWhiteSpace(firstName) ? "Admin" : firstName,
                                       string.IsNullOrWhiteSpace(lastName) ? "User" : lastName,
                                       new Email(email),
                                       password,
                                       employeeManager.PasswordHasher);

        // CreateAsync sets NormalizedUserName/NormalizedEmail (so login's FindByEmailAsync works) but the
        // custom EmployeeUserStore has AutoSaveChanges = false, so it only tracks the entity — persist it.
        var result = await employeeManager.CreateAsync(employee);
        if (result.Succeeded)
        {
            await db.SaveChangesAsync();
        }
    }
}
