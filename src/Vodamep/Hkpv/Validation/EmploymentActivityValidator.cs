using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Data;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class EmploymentActivityValidator : AbstractValidator<HkpvReport>
    {
        /// <summary>
        /// Resultat für gruppierte Fehler pro Mitarbeiter
        /// </summary>
        class StaffResult
        {
            /// <summary>
            /// Kleinstes Datum mit Fehlern
            /// </summary>
            public Nullable<DateTime> MinDate { get; set; }

            /// <summary>
            /// Größtes Datum mit Fehlern
            /// </summary>
            public Nullable<DateTime> MaxDate { get; set; }

            /// <summary>
            /// Mitarbeiter
            /// </summary>
            public Staff Staff { get; set; }

            /// <summary>
            /// Anzahl der Aktivitäten
            /// </summary>
            public int CountOfUnemployedActivities { get; set; }
        }



        public EmploymentActivityValidator()
            : base()
        {

            #region Documentation
            // AreaDef: HKP
            // OrderDef: 03
            // SectionDef: Anstellung
            // StrengthDef: Fehler

            // Fields: Mitarbeiter, Check: Leistungsdatum, Remark: Leistungen außerhalb des Anstellungsverhältnisses, Group: Inhaltlich
            #endregion

            //corert kann derzeit nicht mit AnonymousType umgehen. Vielleicht später:  new  { x.Activities, x.Staffs }
            this.RuleFor(x => x)
                .Custom((x, ctx) =>
               {
                   HkpvReport report = ctx.InstanceToValidate;

                   var activities = report.Activities;
                   DateTime from = report.FromD;
                   DateTime to = report.ToD;

                   // Dictionary von Staffs mit Resultat
                   Dictionary<string, StaffResult> resultDict = new Dictionary<string, StaffResult>();
                   foreach (Staff staff in report.Staffs)
                   {
                       if (!resultDict.ContainsKey(staff.Id))
                           resultDict.Add(staff.Id, new StaffResult()
                           {
                               Staff = staff
                           });

                   }

                   // Alle Activities prüfen
                   foreach (Activity activity in activities)
                   {
                       if (resultDict.ContainsKey(activity.StaffId))
                       {
                           bool insideRange = false;
                           bool skipCheck = false;

                           StaffResult staffResult = resultDict[activity.StaffId];
                           EmploymentValidator employmentVal = new EmploymentValidator(from, to, staffResult.Staff);
                           ActivityValidator activityVal = new ActivityValidator(from, to);

                           foreach (Employment employment in staffResult.Staff.Employments)
                           {
                               ValidationResult employmentResult = employmentVal.Validate(employment);
                               ValidationResult activityResult = activityVal.Validate(activity);

                               if (employmentResult.IsValid &&
                                   activityResult.IsValid)
                               {
                                   if (activity.DateD >= employment.FromD &&
                                       activity.DateD <= employment.ToD)
                                   {
                                       insideRange = true;
                                       break;
                                   }
                               }
                               else
                               {
                                   skipCheck = true;
                                   break;
                               }
                           }

                           if (!insideRange && !skipCheck)
                           {
                               staffResult.CountOfUnemployedActivities++;

                               if (staffResult.MinDate == null ||
                                   activity.DateD < staffResult.MinDate)
                               {
                                   staffResult.MinDate = activity.DateD;
                               }

                               if (staffResult.MaxDate == null ||
                                   activity.DateD > staffResult.MaxDate)
                               {
                                   staffResult.MaxDate = activity.DateD;
                               }
                           }
                       }
                   }


                   // Jeden Mitarbeiter nur einmal als Fehler ausgeben
                   List<StaffResult> errors = resultDict.Values.Where(s => s.CountOfUnemployedActivities > 0).ToList();
                   foreach (StaffResult staffResult in errors)
                   {
                       var index = report.Staffs.IndexOf(staffResult.Staff);

                       ctx.AddFailure(new ValidationFailure($"{nameof(HkpvReport.Staffs)}[{index}]", Validationmessages.InvalidEmploymentForActivity(staffResult.Staff, staffResult.CountOfUnemployedActivities, staffResult.MinDate.Value, staffResult.MaxDate.Value)));
                   }

               });
        }
    }
}
