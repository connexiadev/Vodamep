
using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class AdmissionsValidator : AbstractValidator<StatLpReport>
    {       
        public AdmissionsValidator()
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 04
            // SectionDef: Aufnahme
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Person
            #endregion


            // Zu jeder Admission muss es die Person geben
            this.RuleFor(x => new { x.Persons, x.Admissions })
                .Custom((data, ctx) =>
                {
                    StatLpReport report = ctx.InstanceToValidate as StatLpReport;

                    var idPersons = data.Persons.Select(x => x.Id).ToArray();

                    foreach (var a in data.Admissions.Where(x => !idPersons.Contains(x.PersonId)))
                    {
                        var index = data.Admissions.IndexOf(a);
                        ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Admissions)}[{index}]", Validationmessages.PersonIsNotAvailable(report.GetPersonName(a.PersonId))));
                    }
                });          
        }
    }
}
