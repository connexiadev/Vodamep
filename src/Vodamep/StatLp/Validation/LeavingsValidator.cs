
using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class LeavingsValidator : AbstractValidator<StatLpReport>
    {       
        public LeavingsValidator()
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 05
            // SectionDef: Abgang
            // StrengthDef: Fehler

            // CheckDef: Muss Feld
            // Fields: Person
            #endregion


            // Zu jedem Leaving muss es die Person geben
            this.RuleFor(x => new { x.Persons, x.Leavings })
                .Custom((data, ctx) =>
                {
                    StatLpReport report = ctx.InstanceToValidate as StatLpReport;

                    var idPersons = data.Persons.Select(x => x.Id).ToArray();

                    foreach (var a in data.Leavings.Where(x => !idPersons.Contains(x.PersonId)))
                    {
                        var index = data.Leavings.IndexOf(a);
                        ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Leavings)}[{index}]", Validationmessages.PersonIsNotAvailable(report.GetPersonName(a.PersonId))));
                    }
                });

        }
    }
}
