using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Vodamep.Agp.Model;
using Vodamep.Agp.Validation;
using Vodamep.Data.Dummy;
using Enum = System.Enum;

namespace Vodamep.Specs.Agp.StepDefinitions
{

    [Binding]
    public class AgpValidationSteps
    {
        private readonly ReportContext _context;

        public AgpValidationSteps(ReportContext context)
        {
            if (context.Report == null)
            {
                InitContext(context);
            }

            _context = context;
        }

        private void InitContext(ReportContext context)
        {
            context.GetPropertiesByType = this.GetPropertiesByType;
            
            var loc = new AgpDisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);

            var date = new DateTime(2021, 05, 01);
            var r = AgpDataGenerator.Instance.CreateAgpReport("", date.Year, date.Month, 1, 1, false, false);
            AddDummyActivity(r, r.Persons[0].Id);
            AddDummyStaffActivity(r);

            context.Report = r;
        }

        private AgpReport Report => _context.Report as AgpReport;

        private IEnumerable<IMessage> GetPropertiesByType(string type)
        {
            return type switch
            {
                nameof(Person) => this.Report.Persons,
                nameof(Institution) => new[] { this.Report.Institution },
                nameof(StaffActivity) => this.Report.StaffActivities,
                nameof(Activity) => this.Report.Activities,
                _ => Array.Empty<IMessage>(),
            };
        }

        [Given(@"es ist ein 'AgpReport'")]
        public void GivenItIsAAgpReport()
        {

        }

        [Given(@"der Id einer AGP-Person ist nicht eindeutig")]
        public void GivenPersonIdNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Id = p0.Id;
            p.Id = p0.Id;
        }

        [Given(@"die Agp-Diagnose\(n\) ist auf '(.*)' gesetzt")]
        public void GivenTheDiagnosisGroupIsSetTo(string value)
        {
            this.Report.Persons[0].Diagnoses.Clear();

            if (value.Contains(','))
            {
                var diagnosis = value.Split(',').Select(x => (DiagnosisGroup)Enum.Parse(typeof(DiagnosisGroup), x));
                this.Report.Persons[0].Diagnoses.AddRange(diagnosis);
            }
            else if (Enum.TryParse(value, out DiagnosisGroup diagnosis))
            {
                this.Report.Persons[0].Diagnoses.Add(diagnosis);
            }
            else if (value == "")
            {
                //nothing do do, already emptied yet
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [Given(@"es werden zusätzliche Reisezeiten für einen AGP-Mitarbeiter eingetragen")]
        public void GivenTravelTimesAreAdded()
        {
            var existingTravelTime = this.Report.StaffActivities.First();
            this.Report.StaffActivities.Add(new StaffActivity
            {
                Id = existingTravelTime.Id,
                Date = existingTravelTime.Date,
                DateD = existingTravelTime.DateD,
                Minutes = 125,
                ActivityType = StaffActivityType.TravelingSa
            });
        }

        [Given(@"es werden zusätzliche Leistungen pro AGP-Klient an einem Tag eingetragen")]
        public void GivenAdditonalActivitiesPerClientAndDay()
        {
            var existingActivity = this.Report.Activities.First();
            existingActivity.Minutes = 125;

            this.Report.Activities.Add(new Activity()
            {
                Id = existingActivity.Id,
                Date = existingActivity.Date,
                DateD = existingActivity.DateD,
                Minutes = 125,
                PersonId = existingActivity.PersonId,
                PlaceOfAction = PlaceOfAction.BasePlace,
                Entries = { ActivityType.GeriatricPsychiatricAt }
            });
        }

        [Given(@"die Leistungstypen '(.*)' sind für eine AGP-Aktivität gesetzt")]
        public void GivenTheActivitiyTypesAreSetTo(string value)
        {
            this.Report.Activities[0].Entries.Clear();

            if (value.Contains(','))
            {
                var activityTypes = value.Split(',').Select(x => (ActivityType)Enum.Parse(typeof(ActivityType), x));
                this.Report.Activities[0].Entries.AddRange(activityTypes);
            }
            else if (Enum.TryParse(value, out ActivityType activityType))
            {
                this.Report.Activities[0].Entries.Add(activityType);
            }
            else if (value == "")
            {
                //nothing do do, already emptied yet
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [Given(@"zu einer AGP-Person sind keine AGP-Aktivitäten dokumentiert")]
        public void GivenAPersonWithoutActivities()
        {
            this.Report.Activities[0].PersonId = this.Report.Persons.First().Id + "id";
        }


        private static void AddDummyActivity(AgpReport report, string personId)
        {
            var random = new Random();

            var placeOfAction = ((PlaceOfAction[])(Enum.GetValues(typeof(PlaceOfAction))))
                .Where(x => x != PlaceOfAction.UndefinedPlace)
                .ElementAt(random.Next(Enum.GetValues(typeof(PlaceOfAction)).Length - 1));

            var minutes = random.Next(1, 100) * 5;

            var activity = new Activity() { Id = "1", Date = report.From, PersonId = personId, Minutes = minutes, PlaceOfAction = placeOfAction };
            activity.Entries.Add(new[] { ActivityType.ClearingAt, ActivityType.ContactPartnerAt, ActivityType.GuidancePartnerAt });

            report.Activities.Add(activity);
        }


        private void AddDummyStaffActivity(AgpReport report)
        {
            int minutes = 45;

            var activity = new StaffActivity() { Id = "1", Date = report.From, Minutes = minutes, ActivityType = StaffActivityType.NetworkingSa };

            report.StaffActivities.Add(activity);
        }

    }
}
