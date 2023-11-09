using System;
using FluentValidation;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{
    internal class PersonActivityTimeValidator : AbstractValidator<IPersonActivity>
    {
        #region Documentation
        // AreaDef: MOHI
        // OrderDef: 03
        // SectionDef: Leistung
        // StrengthDef: Fehler

        // CheckDef: Pflichtfeld
        // Fields: Leistungszeit

        // CheckDef: Erlaubte Werte
        // Fields: Leistungszeit, Remark: > 15, < 10000
        #endregion

        #region Documentation
        // AreaDef: TB
        // OrderDef: 03
        // SectionDef: Leistung
        // StrengthDef: Fehler

        // CheckDef: Pflichtfeld
        // Fields: Leistungszeit

        // CheckDef: Erlaubte Werte
        // Fields: Leistungszeit, Remark: > 15, < 10000
        #endregion

        public PersonActivityTimeValidator(float minValue, float maxValue)
        {
            this.RuleFor(x => x.Time)
                .GreaterThanOrEqualTo(minValue)
                .WithMessage(x => Validationmessages.ReportBasePersonActivityWrongValue(x.PersonId, $"< {minValue}"));

            this.RuleFor(x => x.Time)
                .LessThanOrEqualTo(maxValue)
                .WithMessage(x => Validationmessages.ReportBasePersonActivityWrongValue(x.PersonId,$"> {maxValue}"));
        }

        public PersonActivityTimeValidator(DateTime reportDate, float minValue, float maxValue)
        {
            this.RuleFor(x => x.Time)
                .GreaterThanOrEqualTo(minValue)
                .WithMessage(x => Validationmessages.ReportBasePersonActivityWrongValue(x.PersonId, reportDate.ToShortDateString(), $"< {minValue}"));

            this.RuleFor(x => x.Time)
                .LessThanOrEqualTo(maxValue)
                .WithMessage(x => Validationmessages.ReportBasePersonActivityWrongValue(x.PersonId, reportDate.ToShortDateString(), $"> {maxValue}"));
        }
    }
}
