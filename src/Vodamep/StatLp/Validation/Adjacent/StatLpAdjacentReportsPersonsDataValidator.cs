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
    internal class StatLpAdjacentReportsPersonsDataValidator : AbstractValidator<(StatLpReport Predecessor, StatLpReport Report)>
    {

        private static readonly DisplayNameResolver DisplayNameResolver = new DisplayNameResolver();

        static StatLpAdjacentReportsPersonsDataValidator()
        {
            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
        }

        public StatLpAdjacentReportsPersonsDataValidator()
        {
            this.RuleFor(x => x).Custom((data, ctx) =>
            {
                // gemeinsame PersonIds
                var personIds = data.Report.Persons.Select(x => x.Id)
                    .Intersect(data.Predecessor.Persons.Select(x => x.Id))
                    .ToArray();

                CheckBirthday(data, ctx, personIds);
            });
        }

        #region Documentation
        // AreaDef: STAT
        // OrderDef: 01
        // SectionDef: Person
        // StrengthDef: Warnung
        // LocationDef: Eingang
        // Fields: Geburtdatum, Check: Änderung Geburtsdatum, Remark: Gleiche Personen-ID, mehrere Jahrespakete, Group: Inhaltlich
        #endregion

        private void CheckBirthday((StatLpReport Predecessor, StatLpReport Report) data, ValidationContext<(StatLpReport Predecessor, StatLpReport Report)> ctx, string[] personIds)
        {
            var curValues = data.Report.Persons
                   .Where(x => personIds.Contains(x.Id))
                   .Select(x => (x.Id, x.BirthdayD))
                   .Distinct()
                   .ToDictionary(x => x.Id, x => x.BirthdayD);

            var preValues = data.Predecessor.Persons
                .Where(x => personIds.Contains(x.Id))
                .Select(x => (x.Id, x.BirthdayD))
                .Distinct().ToDictionary(x => x.Id, x => x.BirthdayD);

            foreach (var personId in personIds)
            {
                if (!curValues.TryGetValue(personId, out var v1) || !preValues.TryGetValue(personId, out var v2) || v1 != v2)
                {
                    var person = data.Report.Persons.Where(x => x.Id == personId).FirstOrDefault();
                    var index = person != null ? data.Report.Persons.IndexOf(person) : -1;

                    ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Persons)}[{index}]",
                        Validationmessages.PersonsPropertyDiffers(
                            data.Report.GetPersonName(personId),
                            DisplayNameResolver.GetDisplayName(nameof(Person.Birthday)),
                            curValues[personId].ToShortDateString(),
                            preValues[personId].ToShortDateString()
                            ))
                    {
                        Severity = Severity.Warning
                    });
                }
            }
        }

    }
}
