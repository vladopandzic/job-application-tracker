using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Polly;
using Procoding.ApplicationTracker.Application;
using Procoding.ApplicationTracker.Web.Auth;
using Procoding.ApplicationTracker.Web.Controllers;
using Procoding.ApplicationTracker.Web.Localization;
using Procoding.ApplicationTracker.Web.Services;
using Procoding.ApplicationTracker.Web.Services.Interfaces;
using Procoding.ApplicationTracker.Web.ViewModels;
using Procoding.ApplicationTracker.Web.ViewModels.Abstractions;

namespace Procoding.ApplicationTracker.Web.Root;

internal class Program
{
    static async Task Main(string[] args)
    {
        var app = new AppAdapter(args,
                                 typeof(Program),
                                 (services, configuration) =>
                                 {
                                     services.AddServerSideBlazor();

                                     services.AddScoped<ITokenProvider, TokenProvider>();

                                     services.AddControllersWithViews().AddApplicationPart(typeof(AdminController).Assembly);


                                     services.AddScoped<AuthenticationStateProvider, RevalidatingServerAuthenticationState>();
                                     services.AddCascadingAuthenticationState();

                                     services.AddAuthentication().AddCookie(x =>
                                     {
                                         x.LoginPath = "/login";
                                         x.AccessDeniedPath = "/no-access";
                                     });

                                     services.AddAuthorization();

                                     services.AddBlazoredLocalStorage();

                                     services.AddTransient<ISomething, Something>();
                                     services.AddViewModels();
                                     services.AddMudServices(x =>
                                     {
                                         x.SnackbarConfiguration.PreventDuplicates = false;
                                         x.SnackbarConfiguration.MaxDisplayedSnackbars = 20;
                                         x.SnackbarConfiguration.HideTransitionDuration = 100;
                                         x.SnackbarConfiguration.ShowTransitionDuration = 200;
                                     });
                                     services.AddMudBlazorDialog();

                                     var baseApiUrl = configuration.GetValue(typeof(string),"BaseApiUrl") as string;

                                     //Only add retry policy to one of them since all of them have same name
                                     services.AddHttpClient<IJobApplicationSourceService, JobApplicationSourceService>("ServerApi",
                                                                                                                x => x.BaseAddress = new Uri(baseApiUrl))
                                                 .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(3,
                                                                                                                               retryNumber => TimeSpan.FromMilliseconds(600)));

                                     services.AddHttpClient<ICompanyService, CompanyService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));

                                     services.AddHttpClient<ICandidateService, CandidateService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));

                                     services.AddHttpClient<IJobApplicationService, JobApplicationService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));


                                     services.AddHttpClient<IEmployeeService, EmployeeService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));

                                     services.AddHttpClient<IAuthService, AuthService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));

                                     services.AddHttpClient<IWorkLocationTypeService, WorkLocationTypeService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));


                                     services.AddHttpClient<IJobTypeService, JobTypeService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));

                                     services.AddHttpClient<ITranslationService, TranslationService>("ServerApi", x => x.BaseAddress = new Uri(baseApiUrl));

                                     services.AddScoped<LocalizationService>();
                                     services.AddScoped<ServerSideTranslations>();

                                     services.AddTransient<INotificationService, NotificationService>();
                                 });


        await app.StartAsync();
    }
}
