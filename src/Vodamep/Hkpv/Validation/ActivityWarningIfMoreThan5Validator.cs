using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class ActivityWarningIfMoreThan5Validator : AbstractValidator<HkpvReport>
    {
        public ActivityWarningIfMoreThan5Validator()
            : base()
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 04
            // SectionDef: Leistung
            // StrengthDef: Warnung
            // Detail: Leistungen, Check: Gleiche Leistungen, Remark: Max. 5 x die gleiche Leistung pro Person und Tag, Group: Inhaltlich
            #endregion

            RuleFor(x => x.Activities)
                .Custom((list, ctx) =>
                {
                    var moreThan5 = list.Where(x => x.RequiresPersonId())
                                .Select(x => new { Entry = x, Count = x.Entries.GroupBy(g => g).Select(gg => gg.Count()).Max() })
                                .Where(x => x.Count > 5);

                    foreach (var entry in moreThan5)
                    {
                        var index = list.IndexOf(entry.Entry);

                        var f = new ValidationFailure($"{nameof(HkpvReport.Activities)}[{index}]", Validationmessages.ActivityMoreThenFive)
                        {
                            Severity = Severity.Warning
                        };

                        ctx.AddFailure(f);
                    }
                });
        }

    }
}
