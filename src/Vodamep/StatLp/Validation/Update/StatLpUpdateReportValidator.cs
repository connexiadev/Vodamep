using FluentValidation;
using FluentValidation.Results;
using System;
using System.Linq;
using System.Threading;
using Vodamep.StatLp.Model;

namespace Vodamep.StatLp.Validation.Update
{
    internal class StatLpUpdateReportValidator : AbstractValidator<(StatLpReport OldReport, StatLpReport Report)>
    {
        private static readonly DisplayNameResolver DisplayNameResolver = new DisplayNameResolver();

        static StatLpUpdateReportValidator()
        {
            var loc = new DisplayNameResolver();
            ValidatorOptions.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
        }

        #region Documentation
        // AreaDef: STAT
        // OrderDef: 02
        // SectionDef: Hauptmerkmal
        // StrengthDef: Warnung
        // LocationDef: Eingang
        // Fields: Pflegestufe, Check: Wesentliche Änderung, Remark: Gleiche Personen, Pflegestufenänderung > 2 Stunfen, Group: Inhaltlich
        #endregion
        public StatLpUpdateReportValidator()
        {
            this.RuleFor(x => x).Custom((data, ctx) =>
            {
                foreach (var personId in data.OldReport.Persons.Select(x => x.Id))
                {
                    var oldValue = data.OldReport.Attributes
                        .Where(x => x.PersonId == personId)
                        .Where(x => x.ValueCase == Model.Attribute.ValueOneofCase.CareAllowance)
                        .Select(x => (int)x.CareAllowance)
                        .LastOrDefault();

                    var newValue = data.Report.Attributes
                        .Where(x => x.PersonId == personId)
                        .Where(x => x.ValueCase == Model.Attribute.ValueOneofCase.CareAllowance)
                        .Select(x => (int)x.CareAllowance)
                        .LastOrDefault();

                    if (oldValue >= (int)CareAllowance.L1 && oldValue <= (int)CareAllowance.L7
                        && newValue >= (int)CareAllowance.L1 && newValue <= (int)CareAllowance.L7
                        && Math.Abs(oldValue - newValue) > 1)
                    {
                        var attribute = data.Report.Attributes
                            .Where(x => x.PersonId == personId)
                            .Where(x => x.ValueCase == Model.Attribute.ValueOneofCase.CareAllowance)
                            .Last();

                        var index = data.Report.Attributes.IndexOf(attribute);

                        ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Attributes)}[{index}]",
                            $"Die Pflegestufe von '{data.Report.GetPersonName(personId)}' hat sich deutlich von '{DisplayNameResolver.GetDisplayName(((CareAllowance)oldValue).ToString())}' auf '{DisplayNameResolver.GetDisplayName(((CareAllowance)newValue).ToString())}' verändert.")
                        {
                            Severity = Severity.Warning
                        });
                    }

                }
            });
        }
    }
}
