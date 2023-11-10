using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{
    internal class PersonHasOnlyOneActivtyValidator : AbstractValidator<IPerson>
    {
        public PersonHasOnlyOneActivtyValidator(IEnumerable<IPersonActivity> personActivities)
        {
            #region Documentation
            // AreaDef: MOHI
            // OrderDef: 02
            // SectionDef: Leistung
            // StrengthDef: Fehler

            // CheckDef: Erlaubte Werte
            // Fields: Leistungszeit, Remark: Nur ein Eintrag pro Person
            #endregion

            #region Documentation
            // AreaDef: TB
            // OrderDef: 02
            // SectionDef: Leistung
            // StrengthDef: Fehler

            // CheckDef: Erlaubte Werte
            // Fields: Leistungszeit, Remark: Nur ein Eintrag pro Person
            #endregion

            this.RuleFor(x => x)
                .Must(x => { return personActivities.Count(y => y.PersonId == x.Id)  <= 1; })
                .WithMessage(x => Validationmessages.ReportBaseActivityMultipleActivitiesForOnePerson(x.Id));
        }
    }
}