using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class HkpvReportStaffIdValidator : AbstractValidator<HkpvReport>
    {
        public HkpvReportStaffIdValidator()
        {
            this.RuleFor(x => x.Staffs)
                .Custom((list, ctx) =>
                {
                    foreach (var id in list.Select(x => x.Id).OrderBy(x => x).GroupBy(x => x).Where(x => x.Count() > 1))
                    {
                        var item = list.Where(x => x.Id == id.Key).First();
                        var index = list.IndexOf(item);
                        ctx.AddFailure(new ValidationFailure($"{nameof(HkpvReport.Staffs)}[{index}]", Validationmessages.IdIsNotUnique));
                    }
                });

            //corert kann derzeit nicht mit AnonymousType umgehen. Vielleicht später: new { x.Staffs, x.Activities, x.Consultations }
            this.RuleFor(x => new Tuple<IList<Staff>, IEnumerable<Activity>>(x.Staffs, x.Activities))
               .Custom((a, ctx) =>
               {
                   #region Documentation
                   // AreaDef: HKP
                   // OrderDef: 04
                   // SectionDef: Leistung
                   // StrengthDef: Fehler
                   // Fields: Leistungen, Check: Mitarbeiter, Remark: Alle Mitarbeiter müssen mind. 1 Leistung haben
                   #endregion

                   var staffs = a.Item1;
                   var activities = a.Item2;                   
                   

                   var idStaffs = staffs.Select(x => x.Id).Distinct().ToArray();
                   var idActivities = (
                                activities.Select(x => x.StaffId)                                                                
                                ).Distinct().ToArray();

                   foreach (var id in idStaffs.Except(idActivities))
                   {
                       var item = staffs.Where(x => x.Id == id).First();
                       var index = staffs.IndexOf(item);
                       ctx.AddFailure(new ValidationFailure($"{nameof(HkpvReport.Staffs)}[{index}]", Validationmessages.WithoutActivity));

                   }

                   foreach (var id in idActivities.Except(idStaffs))
                   {
                       ctx.AddFailure(new ValidationFailure(nameof(HkpvReport.Staffs), Validationmessages.IdIsMissing(id)));
                   }
               });

        }
    }
}
