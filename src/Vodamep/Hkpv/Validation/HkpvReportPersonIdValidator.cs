using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{

    internal class HkpvReportPersonIdValidator : AbstractValidator<HkpvReport>
    {
        public HkpvReportPersonIdValidator()
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 04
            // SectionDef: Leistung
            // StrengthDef: Hart
            // Detail: Leistungen, Check: Personen, Remark: Alle Personen müssen mind. 1 Leistung haben
            #endregion

            this.RuleFor(x => x).SetValidator(new UniqePersonIdValidator());

            //corert kann derzeit nicht mit AnonymousType umgehen. Vielleicht später: new { x.Persons, x.Activities }
            this.RuleFor(x => new Tuple<IList<Person>, IList<Activity>>(x.Persons, x.Activities))
               .Custom((a, ctx) =>
               {
                   var persons = a.Item1;
                   var activities = a.Item2.Where(x => x.RequiresPersonId()).ToList();

                   var idPersons = persons.Select(x => x.Id).Distinct().ToArray();
                   var idActivities = activities.Select(x => x.PersonId).Distinct().ToArray();

                   foreach (var id in idPersons.Except(idActivities))
                   {
                       var item = persons.Where(x => x.Id == id).First();
                       var index = persons.IndexOf(item);
                       ctx.AddFailure(new ValidationFailure($"{nameof(HkpvReport.Persons)}[{index}]", Validationmessages.WithoutActivity));
                   }

                   foreach (var id in idActivities.Except(idPersons))
                   {
                       var item = activities.Where(x => x.PersonId == id).First();
                       var index = activities.IndexOf(item);
                       ctx.AddFailure(new ValidationFailure($"{nameof(HkpvReport.Activities)}[{index}]", Validationmessages.IdIsMissing(id)));
                   }
               });

        }
    }
}
