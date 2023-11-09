using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using System;
using System.Linq;
using System.Threading;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation.Adjacent
{
    internal class StatLpAdjacentReportsAdmissionsDataValidator : AbstractValidator<(StatLpReport Predecessor, StatLpReport Report)>
    {

        private static readonly DisplayNameResolver DisplayNameResolver = new DisplayNameResolver();

        static StatLpAdjacentReportsAdmissionsDataValidator()
        {
            var loc = new DisplayNameResolver();
            ValidatorOptions.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
        }

        public StatLpAdjacentReportsAdmissionsDataValidator()
        {
            this.RuleFor(x => x).Custom((data, ctx) =>
            {
                // gemeinsame PersonIds
                var personIds = data.Report.Persons.Select(x => x.Id)
                    .Intersect(data.Predecessor.Persons.Select(x => x.Id))
                    .ToArray();

                CheckGenders(data, ctx, personIds);
            });
        }

        #region Documentation
        // AreaDef: STAT
        // OrderDef: 01
        // SectionDef: Person
        // StrengthDef: Warnung
        // LocationDef: Eingang
        // Fields: Geschlecht, Check: Gleiches Geschlecht, Remark: Über mehrere Jahrespakete, Group: Inhaltlich
        #endregion

        private void CheckGenders((StatLpReport Predecessor, StatLpReport Report) data, CustomContext ctx, string[] personIds)
        {
            var curValues = data.Report.Admissions
                   .Where(x => personIds.Contains(x.PersonId))
                   .Select(x => (x.PersonId, x.Gender))
                   .Distinct().ToDictionary(x => x.PersonId, x => x.Gender);

            var preValues = data.Predecessor.Admissions
                .Where(x => personIds.Contains(x.PersonId))
                .Select(x => (x.PersonId, x.Gender))
                .Distinct().ToDictionary(x => x.PersonId, x => x.Gender);

            foreach (var personId in personIds)
            {
                if (!curValues.TryGetValue(personId, out var v1) || !preValues.TryGetValue(personId, out var v2) || v1 != v2)
                {
                    var person = data.Report.Persons.Where(x => x.Id == personId).FirstOrDefault();
                    var index = person != null ? data.Report.Persons.IndexOf(person) : -1;

                    ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Admissions)}",
                        Validationmessages.PersonsPropertyDiffers(
                            data.Report.GetPersonName(personId),
                            DisplayNameResolver.GetDisplayName(nameof(Admission.Gender)),
                            DisplayNameResolver.GetDisplayName(curValues[personId].ToString()),
                            DisplayNameResolver.GetDisplayName(preValues[personId].ToString())
                            ))
                    {
                        Severity = Severity.Warning
                    });
                }
            }
        }


    }
}
