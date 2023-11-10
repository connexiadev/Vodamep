using System.Linq;
using FluentValidation;
using Vodamep.Cm.Model;
using Vodamep.ReportBase;
using Vodamep.ValidationBase;

namespace Vodamep.Cm.Validation
{
    internal class CmClientActivityValidator : AbstractValidator<ClientActivity>
    {
        public CmClientActivityValidator(CmReport report)
        {
            #region Documentation
            // AreaDef: CM
            // OrderDef: 03
            // SectionDef: Klienten-Leistung
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Leistungstyp
            // Detail: Datum
            // Detail: Leistungszeit

            // CheckDef: Erlaubte Werte
            // Detail: Leistungstyp, Remark: Leistungstypen-Liste, Url: src/Vodamep/Datasets/Agp/ActivityType.csv
            // Detail: Datum, Remark: Innerhalb des Meldungs-Zeitraums
            // Detail: Leistungszeit, Remark: > 0, < 10000, in 15-Minuten-Schritten
            #endregion

            this.RuleFor(x => x.ActivityType).NotEmpty().WithMessage(x => Validationmessages.ReportBasePersonActivityNoCategory(x.PersonId, x.Date.ToDateTime().ToShortDateString()));
            this.RuleFor(x => x).SetValidator(x => new PersonActivityTimeValidator(x.Date.ToDateTime(), 0.25f, 10000));

            this.RuleFor(x => x.Date).Must(x => x >= report.From && x <= report.To).WithMessage(x => Validationmessages.ReportBasePersonActivityWrongDate(x.PersonId, x.Date.ToDateTime().ToShortDateString()));

            //ContainsIdValidator check if person is in persons
            this.RuleFor(x => x).SetValidator(new ClientActivityContainsCorrectPersonIdValidator(report.Persons.Select(p => p.Id)));
        }
    }
}