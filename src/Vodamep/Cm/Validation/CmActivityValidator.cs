using FluentValidation;
using Vodamep.Cm.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Cm.Validation
{
    internal class CmActivityValidator : AbstractValidator<Activity>
    {
        public CmActivityValidator(CmReport report)
        {
            #region Documentation
            // AreaDef: CM
            // OrderDef: 02
            // SectionDef: Leistung
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Leistungstyp
            // Detail: Datum
            // Detail: Leistungszeit

            // CheckDef: Erlaubte Werte
            // Detail: Leistungstyp, Remark: Leistungstypen-Liste, Url: src/Vodamep/Datasets/Cm/ActivityType.csv
            // Detail: Datum, Remark: Innerhalb des Meldungs-Zeitraums
            // Detail: Leistungszeit, Remark: > 0, < 10000
            #endregion

            this.RuleFor(x => x.ActivityType).NotEmpty().WithMessage(x => Validationmessages.ReportBaseActivityNoCategory(x.Date.ToDateTime().ToShortDateString()));
            this.RuleFor(x => x.Date).Must(x => x >= report.From && x <= report.To).WithMessage(x => Validationmessages.ReportBaseActivityWrongDate(x.Date.ToDateTime().ToShortDateString()));

            this.RuleFor(x => x.Time)
                .GreaterThanOrEqualTo(1)
                .WithMessage(x => Validationmessages.ReportBaseActivityWrongValue(x.DateD.ToShortDateString(), $"< {1}"));

            this.RuleFor(x => x.Time)
                .LessThanOrEqualTo(10000)
                .WithMessage(x => Validationmessages.ReportBaseActivityWrongValue(x.DateD.ToShortDateString(), $"> {10000}"));

        }
    }
}