using FluentValidation;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Vodamep.Data;
using Vodamep.Data.Dummy;
using Vodamep.Hkpv.Model;
using Vodamep.Hkpv.Validation;

namespace Vodamep.Specs.Hkpv.StepDefinitions
{

    [Binding]
    public class HkpvValidationSteps
    {

        private Activity _dummyActivities;
        private readonly ReportContext _context;

        public HkpvValidationSteps(ReportContext context)
        {
            if (context.Report == null)
            {
                InitContext(context);
                _context = context;
                AddDummyActivities(this.Report.Persons[0].Id, this.Report.Staffs[0].Id);
            }

            _context = context;
        }

        private void InitContext(ReportContext context)
        {
            context.GetPropertiesByType = this.GetPropertiesByType;
            
            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);

            var date = DateTime.Today.AddMonths(-1);
            var r = HkpvDataGenerator.Instance.CreateHkpvReport("", date.Year, date.Month, 1, 1, false);

            var consultation = new Activity() { Date = r.From, StaffId = r.Staffs[0].Id };
            consultation.Entries.Add(ActivityType.Lv31);
            r.Activities.Add(consultation);

            context.Report = r;
        }

        private IEnumerable<IMessage> GetPropertiesByType(string type)
        {
            return type switch
            {
                nameof(Person) =>  this.Report.Persons,
                nameof(Institution) => new[] { this.Report.Institution },
                nameof(Staff) => this.Report.Staffs,
                nameof(Activity) => this.Report.Activities,
                nameof(Employment) => this.Report.Staffs[0].Employments,
                _ => Array.Empty<IMessage>(),
            };
        }

        public HkpvReport Report => _context.Report as HkpvReport;

        [Given(@"es ist ein 'HkpvReport'")]
        public void GivenItIsAHkpvReport()
        {

        }

        [Given(@"die Meldung enthält am '(.*)' die Aktivitäten '(.*)'")]
        public void GivenTheActivitiesAt(string date, string values)
        {
            var d = Timestamp.FromDateTime(date.AsDate());

            this.GivenTheActivitiesAt(this.Report.Persons[0].Id, d, values, this.Report.Staffs[0].Id);
        }

        [Given(@"die Meldung enthält die Anstellungen '(.*)' und die Leistungstage '(.*)'")]
        public void GivenTheEmployments(string employments, string activities)
        {
            string[] fromTos = employments.Split(',');
            Employment existingEmployment = this.Report.Staffs[0].Employments.First();
            

            this.Report.Staffs[0].Employments.Clear();
            this.Report.Activities.Clear();

            string[] activityDates = activities.Split(',');
            foreach (string activityDate in activityDates)
            {
                var a = new Activity() { DateD = new DateTime(this.Report.FromD.Year, this.Report.FromD.Month, Convert.ToInt32(activityDate)), PersonId = this.Report.Persons[0].Id, StaffId = this.Report.Staffs[0].Id };
                a.Entries.Add(ActivityType.Lv01);

                this.Report.Activities.Add(a);
            }

            foreach (string fromTo in fromTos)
            {
                string[] fromToValues = fromTo.Split('-');
                DateTime from = new DateTime(this.Report.FromD.Year, this.Report.FromD.Month, Convert.ToInt32(fromToValues[0]));
                DateTime to = new DateTime(this.Report.FromD.Year, this.Report.FromD.Month, Convert.ToInt32(fromToValues[1]));

                this.Report.Staffs[0].Employments.Add(new Employment()
                {
                    HoursPerWeek = existingEmployment.HoursPerWeek,
                    FromD = from,
                    ToD = to
                });
            }
        }


        [Given(@"die Meldung enthält die Aktivitäten '(.*?)'")]
        public void GivenTheActivities(string values)
        {
            this.GivenTheActivitiesAt(this.Report.Persons[0].Id, this.Report.To, values, this.Report.Staffs[0].Id);
        }

        [Given(@"die Meldung enthält jeden Tag die Aktivitäten '(.*?)'")]
        public void GivenTheDailyActivities(string values)
        {
            this.RemoveDummyActivities();

            foreach (var day in Enumerable.Range(1, this.Report.ToD.Day))
            {
                this.GivenTheActivitiesAt(this.Report.Persons[0].Id, new DateTime(this.Report.ToD.Year, this.Report.ToD.Month, day).AsTimestamp(), values, this.Report.Staffs[0].Id);
            }
        }

        [Given(@"die Meldung enthält bei der Person '(.*)' die Aktivitäten '(.*)'")]
        public void GivenTheActivitiesAtPerson(string personId, string values)
        {
            this.GivenTheActivitiesAt(personId, this.Report.To, values, this.Report.Staffs[0].Id);
        }


        [Given(@"die Meldung enthält von der Mitarbeiterin '(.*)' die Aktivitäten '(.*)'")]
        public void GivenTheActivitiesFromStaff(string staffId, string values)
        {
            this.GivenTheActivitiesAt(this.Report.Persons[0].Id, this.Report.To, values, staffId);
        }

        [Given(@"eine Versicherungsnummer ist nicht eindeutig")]
        public void GivenSsnNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Ssn = p0.Ssn;
        }

        [Given(@"der Id einer Hkpv-Person ist nicht eindeutig")]
        public void GivenPersonIdNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Id = p0.Id;
            p.Id = p0.Id;
        }

        [Given(@"der Id einer Hkpv-Mitarbeiterin ist nicht eindeutig")]
        public void GivenStaffIdNotUnique()
        {
            var s0 = this.Report.Staffs[0];

            var s = this.Report.AddDummyStaff();

            s.Id = s0.Id;
        }

        [Given(@"es ist keine Beschäftigung beim Mitarbeiter vorhanden")]
        public void GivenStaffWithoutEmployment()
        {
            var s0 = this.Report.Staffs[0];
            var s = this.Report.AddDummyStaff();
            this.Report.Staffs[0].Employments.Clear();
        }

        [Given(@"eine Auszubildende hat die Aktivitäten '(.*)' dokumentiert")]
        public void GivenTraineeWithActivity(string values)
        {
            var s = this.Report.AddDummyStaff();
            s.Qualification = QualificationCodeProvider.Instance.Trainee;
            var staffId = s.Id;

            this.GivenTheActivitiesAt(this.Report.Persons[0].Id, this.Report.To, values, staffId);
        }

        [Given(@"zu einer HkpvPerson sind keine Aktivitäten dokumentiert")]
        public void GivenPersonWithoutActivity()
        {
            this.Report.AddDummyPerson();
        }


        [Given(@"zu einer Mitarbeiterin sind keine Aktivitäten dokumentiert")]
        public void GivenStaffWithoutActivity()
        {
            this.Report.AddDummyPerson();
        }

        [Given(@"die Meldung enthält am '(.*)' die Beratungen '(.*)'")]
        public void GivenTheConsultationsAt(string date, string values)
        {
            var d = Timestamp.FromDateTime(date.AsDate());

            this.GivenTheActivitiesAt(string.Empty, d, values, this.Report.Staffs[0].Id);
        }

        private void GivenTheActivitiesAt(string personId, Timestamp d, string values, string staffId)
        {
            this.RemoveDummyActivities();

            var a = new Activity() { Date = d, PersonId = personId, StaffId = staffId };
            a.Entries.AddRange(values.Split(',').Select(x => (ActivityType)int.Parse(x)));

            this.Report.Activities.Add(a);
        }

        private void AddDummyActivities(string personId, string staffId)
        {
            _dummyActivities = new Activity() { Date = this.Report.From, PersonId = personId, StaffId = staffId };
            _dummyActivities.Entries.Add(new[] { ActivityType.Lv02, ActivityType.Lv04, ActivityType.Lv15 });

            this.Report.Activities.Add(_dummyActivities);
        }

        private void RemoveDummyActivities()
        {
            if (_dummyActivities != null)
            {
                this.Report.Activities.Remove(_dummyActivities);
                _dummyActivities = null;
            }
        }
    }
}
