using FluentValidation;
using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.Web.Validators;

public class MyNewJobApplicationValidator : FluentValueValidator<JobApplicationDTO>
{
    public MyNewJobApplicationValidator()
    {

        RuleFor(x => x.Company).NotEmpty().WithMessage("Tvrtka je obavezna.");

        RuleFor(x => x.Company).Custom((company, context) =>
        {
            if (company is not null)
            {
                if (string.IsNullOrEmpty(company.CompanyName) || company.Id == Guid.Empty)
                {
                    context.AddFailure("Odaberi tvrtku s popisa.");
                }
            }
        });

        RuleFor(x => x.ApplicationSource).NotEmpty().WithMessage("Izvor prijave je obavezan.");

        RuleFor(x => x.ApplicationSource).Custom((applicationSource, context) =>
        {
            if (applicationSource is not null)
            {
                if (string.IsNullOrEmpty(applicationSource.Name) || applicationSource.Id == Guid.Empty)
                {
                    context.AddFailure("Odaberi izvor prijave.");
                }
            }
        });
        RuleFor(x => x.JobPositionTitle).NotEmpty().WithMessage("Naziv radnog mjesta je obavezan.");


        RuleFor(x => x.JobAdLink).NotEmpty().WithMessage("Poveznica na oglas je obavezna.").ValidUrl().WithMessage("Unesi ispravnu poveznicu (npr. https://…).");

        RuleFor(x => x.WorkLocationType).NotEmpty().WithMessage("Lokacija rada je obavezna.");

        RuleFor(x => x.WorkLocationType).Custom((workLocation, context) =>
        {
            if (workLocation is not null)
            {
                if (string.IsNullOrEmpty(workLocation.Value))
                {
                    context.AddFailure("Lokacija rada je obavezna.");
                }
            }
        });

        RuleFor(x => x.JobType).NotEmpty().WithMessage("Vrsta posla je obavezna.");

        RuleFor(x => x.JobType).Custom((jobType, context) =>
        {
            if (jobType is not null)
            {
                if (string.IsNullOrEmpty(jobType.Value))
                {
                    context.AddFailure("Vrsta posla je obavezna.");
                }
            }
        });

    }
}
