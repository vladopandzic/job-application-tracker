using FluentValidation;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Procoding.ApplicationTracker.Api.Extensions;
using Procoding.ApplicationTracker.Api.Infrastructure;
using Procoding.ApplicationTracker.Api.Validation;
using Procoding.ApplicationTracker.Application.Authentication;
using Procoding.ApplicationTracker.Application.Authentication.JwtTokens;
using Procoding.ApplicationTracker.Application.Candidates.Commands.UpdateCandidate;
using Procoding.ApplicationTracker.Application.Core.Extensions;
using Procoding.ApplicationTracker.Application.JobApplicationSources.Query.GetJobApplicationSources;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Infrastructure;
using Procoding.ApplicationTracker.Infrastructure.Authentication;
using Procoding.ApplicationTracker.Infrastructure.Data;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
namespace Procoding.ApplicationTracker.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGenWithBearerAuthorization("Bearer");

        builder.Services.AddTransient<IMapper, Mapper>();
        builder.Services.AddMediatR(x =>
        {

            x.RegisterServicesFromAssemblies(typeof(Program).Assembly, typeof(GetJobApplicationSourcesQuery).Assembly)
             .AddHandlerValidations();
        });

        // Database provider is selectable via the "DatabaseProvider" config key ("Postgres" | "SqlServer").
        // Postgres migrations live in the dedicated migrations assembly; SqlServer migrations live in Infrastructure.
        var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Postgres";
        var connectionString = builder.Configuration.GetConnectionString("JobApplicationDatabase");

        builder.Services.AddDbContext<ApplicationDbContext>(option =>
        {
            if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                option.UseSqlServer(connectionString,
                    x => x.MigrationsAssembly("Procoding.ApplicationTracker.Infrastructure"));
            }
            else
            {
                option.UseNpgsql(connectionString,
                    x => x.MigrationsAssembly("Procoding.ApplicationTracker.Persistance.Migrations.PostgreSQL"));
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            }
        });

        builder.Services.AddPersistance();

        builder.Services.AddEmailSending(builder.Configuration);

        builder.Services.AddAiExtraction(builder.Configuration);

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddValidatorsFromAssemblies([typeof(CandidateInsertRequestDTOValidator).Assembly,
                                                      typeof(UpdateCandidateCommand).Assembly]);


        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddIdentityCore<Employee>()
                        .AddUserStore<EmployeeUserStore>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();

        builder.Services.AddIdentityCore<Candidate>()
                        .AddUserStore<CandidateUserStore>()
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();



        builder.Services.AddAuthentication(x =>
        {
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer("BearerEmployee", config =>
        {
            var jwtTokenOptionsEmployee = builder.Configuration.GetSection("EmployeeJwtTokenSettings").Get<JwtTokenOptions<Employee>>()!;

            SecurityKey? secretKey = new JwtTokenCreator<Employee>(jwtTokenOptionsEmployee).GetDefaultSigningCredentials(secretkey: jwtTokenOptionsEmployee!.SecretKey).Key;

            config.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtTokenOptionsEmployee.Issuer,
                ValidAudience = jwtTokenOptionsEmployee.Audience,
                IssuerSigningKey = secretKey,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.Zero,

            };
        }).AddJwtCreator<Employee>(builder.Configuration, "EmployeeJwtTokenSettings")
        .AddJwtBearer("BearerCandidate", config =>
        {
            var jwtTokenOptionsCandidate = builder.Configuration.GetSection("CandidateJwtTokenSettings").Get<JwtTokenOptions<Candidate>>()!;
            SecurityKey? secretKey = new JwtTokenCreator<Candidate>(jwtTokenOptionsCandidate).GetDefaultSigningCredentials(secretkey: jwtTokenOptionsCandidate!.SecretKey).Key;

            config.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = (x) =>
                {
                    return Task.CompletedTask;
                }
            };


            config.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtTokenOptionsCandidate.Issuer,
                ValidAudience = jwtTokenOptionsCandidate.Audience,
                IssuerSigningKey = secretKey,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.Zero,

            };

        }).AddJwtCreator<Candidate>(builder.Configuration, "CandidateJwtTokenSettings");


        // Persist Data Protection keys to disk so Identity tokens (email confirmation, password reset)
        // survive app-pool recycles on IIS/MonsterASP. Without this the key ring is regenerated on
        // restart and previously-sent confirmation links stop validating.
        builder.Services.AddDataProtection()
                        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys")))
                        .SetApplicationName("JobTrek");

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.EmployeeOnly, Policies.EmployeeOnlyPolicy());
            options.AddPolicy(Policies.CandidateOnly, Policies.CandidateOnlyPolicy());

        });
        var app = builder.Build();

        // Apply any pending EF migrations on startup so a fresh (e.g. cloud) database gets its schema
        // automatically — no separate "ef database update" step needed when deploying. Runs the
        // migrations for the configured provider (Postgres / SqlServer). Idempotent.
        using (var migrationScope = app.Services.CreateScope())
        {
            var migrationDb = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await migrationDb.Database.MigrateAsync();
        }

        // Base UI translations (Croatian + English) — seeded on every startup if missing.
        using (var translationScope = app.Services.CreateScope())
        {
            var translationDb = translationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await TranslationSeeder.SeedAsync(translationDb);
        }

        // Production admin bootstrap — creates the admin employee from configuration (secrets), never
        // from hardcoded credentials. No-op if not configured or the account already exists.
        using (var adminScope = app.Services.CreateScope())
        {
            var adminDb = adminScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var adminEmployeeManager = adminScope.ServiceProvider.GetRequiredService<UserManager<Employee>>();
            var adminSection = app.Configuration.GetSection("SeedAdmin");
            await AdminSeeder.SeedAsync(adminDb,
                                        adminEmployeeManager,
                                        adminSection["Email"],
                                        adminSection["Password"],
                                        adminSection["FirstName"],
                                        adminSection["LastName"]);
        }

        // Development-only demo data so the UI (Kanban board) has something to show.
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var seedDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var candidateManager = scope.ServiceProvider.GetRequiredService<UserManager<Candidate>>();
            var employeeManager = scope.ServiceProvider.GetRequiredService<UserManager<Employee>>();
            var seedTimeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
            await DevDataSeeder.SeedAsync(seedDb, candidateManager, employeeManager, seedTimeProvider);
        }

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseHttpsRedirection();

        app.UseExceptionHandler();

        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllers();

        // TEMP diagnostic — raw Gemini call to surface the real error (model/key/quota). Remove after testing.
        app.MapGet("/diag/extract", async (IConfiguration cfg, CancellationToken ct) =>
        {
            var key = cfg["Gemini:ApiKey"];
            var model = cfg["Gemini:Model"] ?? "gemini-2.0-flash";
            if (string.IsNullOrWhiteSpace(key))
            {
                return Results.Text("NO KEY in config (Gemini:ApiKey empty)");
            }

            var body = new
            {
                contents = new[] { new { parts = new[] { new { text = "Return a JSON object {\"ok\":true}." } } } },
                generationConfig = new { responseMimeType = "application/json" }
            };

            string[] candidates =
            {
                "gemini-2.5-flash", "gemini-2.5-flash-lite", "gemini-2.0-flash-exp",
                "gemini-1.5-flash", "gemini-flash-latest", "gemini-2.0-flash"
            };

            using var http = new HttpClient();
            var lines = new List<string>();
            foreach (var m in candidates)
            {
                try
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={key}";
                    using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(http, url, body, ct);
                    lines.Add($"{m} -> {(int)resp.StatusCode} {resp.StatusCode}");
                }
                catch (Exception ex)
                {
                    lines.Add($"{m} -> EX {ex.Message}");
                }
            }
            return Results.Text(string.Join("\n", lines));
        });

        app.Run();
    }

    private static async Task<ApplicationDbContext> SeedDatabaseAsync(WebApplication app)
    {
        var context = app.Services.GetRequiredScopedService<ApplicationDbContext>();
        var passwordHasher = app.Services.GetRequiredScopedService<IPasswordHasher<Candidate>>();
        await SeedData.SeedAsync(context, passwordHasher);
        return context;
    }

    private static async Task<ApplicationDbContext> SeedOneEmployee(WebApplication app)
    {
        var context = app.Services.GetRequiredScopedService<ApplicationDbContext>();
        var userManager = app.Services.GetRequiredScopedService<UserManager<Employee>>();
        await SeedData.SeedEmployee(context, userManager);
        return context;
    }
}

