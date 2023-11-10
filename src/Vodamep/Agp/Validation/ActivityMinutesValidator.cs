using FluentValidation;
using FluentValidation.Results;
using Vodamep.Agp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Agp.Validation
{
    internal class ActivityMinutesValidator : AbstractValidator<Activity>
    {
        public ActivityMinutesValidator(string propertyName, string client)
        {
            #region Documentation
            // AreaDef: AGP
            // OrderDef: 03
            // SectionDef: Klienten-Leistung
            // StrengthDef: Hart

            // CheckDef: Erlaubte Werte
            // Detail: Leistungszeit, Remark: > 0, in 5-Minuten-Schritten
            #endregion

            this.RuleFor(x => x.Minutes)
                .GreaterThan(0)
                .WithMessage(x => Validationmessages.ReportBaseMinutesMustBeGreater0(propertyName, client));

            this.RuleFor(x => x)
                .Custom((activity, ctx) =>
                {
                    var activityMinutes = activity.Minutes;

                    if (activityMinutes > 0 && activityMinutes % 5 != 0)
                    {
                        ctx.AddFailure(new ValidationFailure(propertyName, Validationmessages.ReportBaseStepWidthWrong(propertyName, client, 5)));
                    }
                });
        }
    }
}