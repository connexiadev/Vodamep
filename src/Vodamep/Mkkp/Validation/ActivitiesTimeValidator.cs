using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Mkkp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Mkkp.Validation
{
    internal class ActivitiesTimeValidator : AbstractValidator<MkkpReport>
    {
        public ActivitiesTimeValidator()
        {

            // Max. 12 Std. pro Mitarbeiter pro Tag

            this.RuleFor(x => x).Custom((x, ctx) =>
            {
                //10 hours max
                int maxNoOfMinutes = 12 * 60;

                Dictionary<string, Activity> timeDictionary = new Dictionary<string, Activity>();

                foreach (Activity activity in x.Activities)
                {
                    string key = activity.StaffId + activity.DateD.ToShortDateString();
                    if (!timeDictionary.ContainsKey(key))
                    {
                        // Summieren mit Clone, sonst wird das Original Objekt mit der Prüfunge geändert
                        Activity sum = activity.Clone();
                        timeDictionary.Add(key, sum);
                    }
                    else
                    {
                        timeDictionary[key].Minutes += activity.Minutes;
                    }
                }

                foreach (KeyValuePair<string, Activity> keyValue in timeDictionary)
                {
                    if (keyValue.Value.Minutes > maxNoOfMinutes)
                    {
                        string name = x.GetStaffName(keyValue.Value.StaffId);
                        ctx.AddFailure(new ValidationFailure(nameof(MkkpReport.Activities), Validationmessages.ReportBaseMaxSumOfMinutesPerStaffMemberIs12Hours(keyValue.Value.DateD.ToShortDateString(), name)));
                    }
                }

            });



            // Max. 5 Std. pro Mitarbeiter pro Tag Fahrzeit

            this.RuleFor(x => x)
                .Custom((report, ctx) =>
                {
                    //5 hours max
                    int maxNoOfMinutes = 5 * 60;

                    var staffs = report.Staffs;
                    var travelTimes = report.TravelTimes;

                    var tavelTimesPerStaffId = travelTimes.GroupBy(y => y.StaffId)
                        .Select((group) => new { Key = group.Key, Items = group.ToList() });

                    foreach (var travelTimeStaffId in tavelTimesPerStaffId)
                    {
                        var travelTimesPerStaffIdAndDate = travelTimeStaffId.Items.GroupBy(z => z.DateD)
                            .Select(group => new { Date = group.Key, Items = group.ToList() });

                        foreach (var tt in travelTimesPerStaffIdAndDate)
                        {
                            var sumOfMinutes = tt.Items.Sum(x => x.Minutes);

                            if (sumOfMinutes > maxNoOfMinutes)
                            {
                                var staff = staffs.FirstOrDefault(x => x.Id == travelTimeStaffId.Key);

                                ctx.AddFailure(new ValidationFailure(nameof(MkkpReport.Activities), Validationmessages.MaxSumOfMinutesTravelTimesIs5Hours(staff.GetDisplayName(), tt.Date.ToShortDateString())));
                            }
                        }
                    }
                });
        }
    }

}
