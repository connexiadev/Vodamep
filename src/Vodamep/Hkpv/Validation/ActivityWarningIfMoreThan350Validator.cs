using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class ActivityWarningIfMoreThan350Validator : AbstractValidator<HkpvReport>
    {
        public ActivityWarningIfMoreThan350Validator()
            : base()
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 04
            // SectionDef: Leistung
            // StrengthDef: Fehler
            // Fields: Leistungen, Check: Leistungspunkte, Remark: Max. 350 Leistungspunkte pro Person/Monat, Group: Inhaltlich
            #endregion

            this.RuleFor(x => new Tuple<IList<Activity>, IEnumerable<Person>>(x.Activities, x.Persons))
                .Custom((a, ctx) =>
                {
                    var moreThan350 = a.Item1.Where(x => x.PersonId != string.Empty)
                        .GroupBy(x => x.PersonId)
                        .Select(x => new { PersonId = x.Key, Sum = x.Sum(y => y.GetLP()) })
                        .Where(x => x.Sum > 350);

                    foreach (var entry in moreThan350)
                    {
                        var p = a.Item2.Where(x => x.Id == entry.PersonId).FirstOrDefault();

                        var f = new ValidationFailure($"{nameof(HkpvReport)}", Validationmessages.ActivityMoreThen350(p, entry.Sum))
                        {
                            Severity = Severity.Warning
                        };

                        ctx.AddFailure(f);
                    }
                });
        }

    }
}
