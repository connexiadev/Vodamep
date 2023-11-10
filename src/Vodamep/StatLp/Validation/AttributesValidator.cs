
using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{

    internal class AttributesValidator : AbstractValidator<StatLpReport>
    {
        private static readonly DisplayNameResolver DisplayNameResolver = new DisplayNameResolver();

        public AttributesValidator()
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 02
            // SectionDef: Hauptmerkmal
            // StrengthDef: Fehler

            // CheckDef: Erlaubte Werte
            // Fields: Finanzierung/Pflegestufen, Remark: Keine Duplikate bei Person/Datum/Finanzierung/Pflegestufen

            #endregion


            // Zu jedem Attribut muss es die Person geben
            this.RuleFor(x => new { x.Persons, x.Attributes })
                .Custom((a, ctx) =>
                {
                    StatLpReport report = ctx.InstanceToValidate as StatLpReport;

                    var idPersons = a.Persons.Select(x => x.Id).ToArray();

                    foreach (var attr in a.Attributes.Where(x => !idPersons.Contains(x.PersonId)))
                    {
                        var index = a.Attributes.IndexOf(attr);
                        ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Attributes)}[{index}]", Validationmessages.PersonIsNotAvailable(report.GetPersonName(attr.PersonId))));
                    }
                });


            // keine Duplikate
            this.RuleFor(x => x.Attributes)
                .Custom((attributes, ctx) =>
            {
                var report = ctx.InstanceToValidate as StatLpReport;

                foreach (var a in attributes
                    .GroupBy(x => (x.PersonId, x.ValueCase, x.FromD))
                    .Where(x => x.Count() > 1))
                {
                    ctx.AddFailure(new ValidationFailure(nameof(StatLpReport.Admissions),
                                        Validationmessages.StatLpAttributeMultiple(
                                            report.GetPersonName(a.Key.PersonId),
                                            a.Key.FromD.ToShortDateString(),
                                            DisplayNameResolver.GetDisplayName(a.Key.ValueCase.ToString()))));
                }
            });
        }
    }
}
