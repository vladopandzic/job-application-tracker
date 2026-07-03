using FluentValidation;
using Procoding.ApplicationTracker.DTOs.Model;

namespace Procoding.ApplicationTracker.Web.Validators;

public class CompanyValidator : FluentValueValidator<CompanyDTO>
{
    public CompanyValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().WithMessage("Naziv tvrtke je obavezan.");

        RuleFor(x => x.OfficialWebSiteLink).NotEmpty().WithMessage("Poveznica na web stranicu je obavezna.").ValidUrl();

    }
}
