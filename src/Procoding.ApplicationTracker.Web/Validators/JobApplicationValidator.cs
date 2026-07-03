using FluentValidation;
using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.Web.Validators;

public class JobApplicationValidator : Validators.FluentValueValidator<JobApplicationDTO>
{
    public JobApplicationValidator()
    {
        RuleFor(x => x.Candidate).NotEmpty().WithMessage("Kandidat je obavezan.");

        RuleFor(x => x.Company).NotEmpty().WithMessage("Tvrtka je obavezna.");

        RuleFor(x => x.ApplicationSource).NotEmpty().WithMessage("Izvor prijave je obavezan.");


        RuleFor(x => x.JobPositionTitle).NotEmpty().WithMessage("Naziv radnog mjesta je obavezan.");


        RuleFor(x => x.JobAdLink).NotEmpty().WithMessage("Poveznica na oglas je obavezna.");

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
