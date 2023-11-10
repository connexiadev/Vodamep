using System.Globalization;
using FluentValidation;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{
    internal class ActivityStepLengthValidator : AbstractValidator<IPersonActivity>
    {
        #region Documentation
        // AreaDef: MOHI
        // OrderDef: 03
        // SectionDef: Leistung
        // StrengthDef: Fehler

        // CheckDef: Erlaubte Werte
        // Fields: Leistungszeit, Remark: In 15-Minuten-Schritten
        #endregion

        #region Documentation
        // AreaDef: TB
        // OrderDef: 03
        // SectionDef: Leistung
        // StrengthDef: Fehler

        // CheckDef: Erlaubte Werte
        // Fields: Leistungszeit, Remark: In 15-Minuten-Schritten
        #endregion

        public ActivityStepLengthValidator(float stepLength)
        {
            this.RuleFor(x => x.Time)
                .Must(x => x % stepLength < float.Epsilon)
                .WithMessage(x => Validationmessages.ReportBaseActivityWrongStepLength(x.PersonId, stepLength.ToString("F2", CultureInfo.InvariantCulture)));
        }

    }
}