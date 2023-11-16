using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Vodamep.Data.Dummy;
using Vodamep.Mkkp.Model;
using Vodamep.Mkkp.Validation;
using Enum = System.Enum;

namespace Vodamep.Specs.Mkkp.StepDefinitions
{

    [Binding]
    public class MkkpValidationSteps
    {

        private readonly ReportContext _context;

        public MkkpValidationSteps(ReportContext context)
        {
            if (context.Report == null)
            {
                InitContext(context);
            }

            _context = context;
        }

        private void InitContext(ReportContext context)
        {
            context.GetPropertiesByType = GetPropertiesByType;
            
            var loc = new MkkpDisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);

            var date = new DateTime(2021, 05, 01);
            var r = MkkpDataGenerator.Instance.CreateMkkpReport("", date.Year, date.Month, 1, 1, false, false);

            this.AddDummyActivity(r, r.Persons[0].Id, r.Staffs[0].Id);

            context.Report = r;
        }

        private IEnumerable<IMessage> GetPropertiesByType(string type)
        {
            return type switch
            {
                nameof(Person) => this.Report.Persons,
                nameof(Institution) => new[] { this.Report.Institution },
                nameof(Activity) => this.Report.Activities,
                nameof(Staff) => this.Report.Staffs,
                nameof(TravelTime) => this.Report.TravelTimes,
                _ => Array.Empty<IMessage>(),
            };
        }

        public MkkpReport Report => _context.Report as MkkpReport;

        [Given(@"es ist ein 'MkkpReport'")]
        public void GivenItIsAHkpvReport()
        {

        }


        [Given(@"der Id einer Mkkp-Person ist nicht eindeutig")]
        public void GivenPersonIdNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Id = p0.Id;
            p.Id = p0.Id;
        }



        [Given(@"der Id einer Mkkp-Mitarbeiterin ist nicht eindeutig")]
        public void GivenStaffIdNotUnique()
        {
            var s0 = this.Report.Staffs[0];

            var s = this.Report.AddDummyStaff(true);

            s.Id = s0.Id;
        }


        [Given(@"die Mkkp-Diagnose\(n\) ist auf '(.*)' gesetzt")]
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

        [Given(@"es werden zusätzliche Wegzeiten für einen Mkkp-Mitarbeiter eingetragen")]
        public void GivenTravelTimesAreAdded()
        {
            var existingTravelTime = this.Report.TravelTimes.FirstOrDefault();

            this.Report.TravelTimes.Add(new TravelTime
            {
                Id = existingTravelTime.Id,
                Date = existingTravelTime.Date,
                DateD = existingTravelTime.DateD,
                Minutes = 125,
                StaffId = existingTravelTime.StaffId
            });
        }

        [Given(@"es werden zusätzliche Leistungen pro Mkkp-Klient an einem Tag eingetragen")]
        public void GivenAdditonalActivitiesPerClientAndDay()
        {
            var existingActivity = this.Report.Activities.First();
            existingActivity.Minutes = 250;

            this.Report.Activities.Add(new Activity()
            {
                Id = existingActivity.Id,
                Date = existingActivity.Date,
                DateD = existingActivity.DateD,
                Minutes = 125,
                StaffId = existingActivity.StaffId,
                PersonId = existingActivity.PersonId,
                PlaceOfAction = PlaceOfAction.ResidencePlace,
                Entries = { ActivityType.MedicalDiet }
            });
        }

        [Given(@"die Leistungstypen '(.*)' sind für eine Mkkp-Aktivität gesetzt")]
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

        [Given(@"zu einer Mkkp-Person sind keine Aktivitäten dokumentiert")]
        public void GivenAPersonWithoutActivities()
        {
            this.Report.Activities[0].PersonId = this.Report.Persons.First().Id + "id";
        }

        [Given(@"zu einer Mkkp-Mitarbeiterin sind keine Aktivitäten dokumentiert")]
        public void GivenAStaffMemberWithoutActivities()
        {
            this.Report.Activities[0].StaffId = this.Report.Staffs.First().Id + "id";
        }

        private void AddDummyActivity(MkkpReport r, string personId, string staffId)
        {
            var random = new Random();

            var placeOfAction = ((PlaceOfAction[])(Enum.GetValues(typeof(PlaceOfAction))))
                .Where(x => x != PlaceOfAction.UndefinedPlace)
                .ElementAt(random.Next(Enum.GetValues(typeof(PlaceOfAction)).Length - 1));

            var minutes = random.Next(1, 100) * 5;
            var dummyActivity = new Activity() { Id = "1", Date = r.From, PersonId = personId, StaffId = staffId, Minutes = minutes, PlaceOfAction = placeOfAction };
            dummyActivity.Entries.Add(new[] { ActivityType.Body, ActivityType.MedicalDiet, ActivityType.MedicalWound });

            r.Activities.Add(dummyActivity);
        }

    }
}
