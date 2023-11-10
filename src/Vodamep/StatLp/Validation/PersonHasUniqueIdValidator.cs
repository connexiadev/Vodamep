using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class PersonHasUniqueIdValidator : AbstractValidator<StatLpReport>
    {
        public PersonHasUniqueIdValidator()
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Fehler

            // CheckDef: Eindeutigkeit
            // Fields: Name/Geburtsdatum, Remark: Kein gleicher Name/Geburtstag, wenn nicht speziell deklariert, Group: Inhaltlich
            #endregion

            this.RuleFor(x => new { x.Persons, x.Aliases })
                .Custom((item, ctx) =>
                {

                    var aliases = item.Aliases.Select(x => x.Id1).Union(item.Aliases.Select(x => x.Id2)).Distinct().ToList();
                    var persons = item.Persons;

                    foreach (var entry in persons
                        .GroupBy(x => (x.FamilyName, x.GivenName, x.Birthday))
                        .Where(x => x.Count() > 1))
                    {
                        // nur ids, für die kein alias-Eintrag vorhanden ist.
                        var ids = entry
                            .Select(x => x.Id)
                            .Where(x => !aliases.Contains(x))
                            .ToList();

                        if (ids.Any())
                        {
                            var index = persons.IndexOf(entry.First());
                            ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Persons)}[{index}]", Validationmessages.PersonWithMultipleIds(entry.First(), ids)));
                        }
                    }
                });
        }
    }
}
