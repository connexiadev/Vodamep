using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class ActivityValidator4141617Without123 : AbstractValidator<Activity>
    {
        public ActivityValidator4141617Without123(IEnumerable<Person> persons, IEnumerable<Staff> staffs)
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 04
            // SectionDef: Leistung
            // StrengthDef: Fehler
            // Fields: Leistungen, Check: Hausbesuch, Remark: Eine Leistung 4-14, 16-17 ohne Hausbesuch oder Beratung, Group: Inhaltlich
            #endregion


            RuleFor(x => x.Entries)
                .Custom((list, ctx) =>
                {
                    if (!list?.Any() ?? false)
                        return;

                    var l = list;

                    var activtiy = ctx.ParentContext.InstanceToValidate as Activity;
                    string person = string.Empty;
                    string staff = string.Empty;

                    if (!string.IsNullOrWhiteSpace(activtiy.PersonId))
                    {
                        var p = persons.FirstOrDefault(x => x.Id == activtiy.PersonId);
                        if (p != null)
                            person = $"{p.GivenName} {p.FamilyName}";
                    }

                    if (!string.IsNullOrWhiteSpace(activtiy.StaffId))
                    {
                        var s = staffs.FirstOrDefault(x => x.Id == activtiy.StaffId);
                        if (s != null)
                            staff = $"{s.GivenName} {s.FamilyName}";
                    }

                    var entries123 = l.Where(x => x == ActivityType.Lv01 || x == ActivityType.Lv02 || x == ActivityType.Lv03).Any();

                    var entries4Except15 = l.Where(x => x != ActivityType.Lv15 && ((int)x > 3) && ((int)x <= 17)).Any();

                    if (entries4Except15 && !entries123)
                    {
                        ctx.AddFailure(new ValidationFailure($"{nameof(Activity.Entries)}", Validationmessages.WithoutEntry("1,2,3", person, staff, activtiy.DateD.ToShortDateString())));
                    }
                });
        }
    }
}
