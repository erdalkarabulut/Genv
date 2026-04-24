using FluentValidation;

namespace Application.Features.ClinicalConfiguration.Commands.UpdateClinicalSettings;

public class UpdateClinicalSettingsCommandValidator : AbstractValidator<UpdateClinicalSettingsCommand>
{
    public UpdateClinicalSettingsCommandValidator()
    {
        RuleFor(x => x.SessionCd34Cd3Divisor).GreaterThan(0);
        RuleFor(x => x.DliCd3CalculationDivisor).GreaterThan(0);
        RuleFor(x => x.MaxApheresisDaysAutologous).GreaterThanOrEqualTo(1);
        RuleFor(x => x.MaxApheresisDaysAllogeneic).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TargetCd34AutologousPerKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TargetCd34AllogeneicPerKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.IdealCd34AutologousPerKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.IdealCd34AllogeneicPerKg).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DliHighDoseCd3PerKgThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProductMinimumCd34PerKg).GreaterThanOrEqualTo(0);
    }
}
