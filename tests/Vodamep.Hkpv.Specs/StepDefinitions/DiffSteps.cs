using FluentValidation;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using Vodamep.Data.Dummy;
using Vodamep.Hkpv.Model;
using Vodamep.Hkpv.Validation;
using Vodamep.ReportBase;
using Xunit;

namespace Vodamep.Specs.StepDefinitions
{

    [Binding]
    public class DiffSteps
    {

        public DiffSteps()
        {
            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
        }

        public HkpvReport Report1 { get; private set; }

        public HkpvReport Report2 { get; private set; }

        [BeforeScenario()]
        public void BeforeScenario()
        {
            this.Report1 = new HkpvReport();
            this.Report2 = new HkpvReport();
        }


        [Given(@"die Meldung1 enthält die Klienten '(.*?)' mit Leistungen")]
        public void GivenReport1AddPersons(string values)
        {
            this.GivenReportAddPersons(this.Report1, values);
        }

        [Given(@"die Meldung1 enthält den Klienten '(.*?)' mit '(.*)' Klientenleistungen")]
        public void GivenReport1AddPerson(string id, int nrOfActivities)
        {
            this.GivenReportAddPerson(this.Report1, id, nrOfActivities);
        }

        [Given(@"die Meldung2 enthält die Klienten '(.*?)' mit Leistungen")]
        public void GivenReport2AddPersons(string values)
        {
            this.GivenReportAddPersons(this.Report2, values);
        }

        [Given(@"die Meldung2 enthält den Klienten '(.*?)' mit '(.*)' Klientenleistungen")]
        public void GivenReport2AddPerson(string id, int nrOfActivities)
        {
            this.GivenReportAddPerson(this.Report2, id, nrOfActivities);
        }

        [Given(@"die Meldung1 enthält den Mitarbeiter '(.*?)' mit '(.*)' Mitarbeiterleistungen und mit '(.*)' Anstellungen mit '(.*)' Stunden pro Woche")]
        public void GivenReport1AddStaff(string staffId, int nrOfStaffActivities, int nrOfEmployments, float employments)
        {
            this.GivenReportAddStaff(this.Report1, staffId, nrOfStaffActivities, nrOfEmployments, employments);
        }

        [Given(@"die Meldung2 enthält den Mitarbeiter '(.*?)' mit '(.*)' Mitarbeiterleistungen und mit '(.*)' Anstellungen mit '(.*)' Stunden pro Woche")]
        public void GivenReport2AddStaff(string staffId, int nrOfStaffActivities, int nrOfEmployments, float employments)
        {
            this.GivenReportAddStaff(this.Report2, staffId, nrOfStaffActivities, nrOfEmployments, employments);
        }

        [Given(@"die Eigenschaft '(\w*)' von '(\w*)' von Report 1 ist auf '(.*)' gesetzt")]
        public void GivenThePropertyReport1IsSetTo(string name, string type, string value)
        {
            this.GivenThePropertyIsSetTo(this.Report1, name, type, value);
        }

        [Given(@"die Eigenschaft '(\w*)' von '(\w*)' von Report 2 ist auf '(.*)' gesetzt")]
        public void GivenThePropertyReport2IsSetTo(string name, string type, string value)
        {
            this.GivenThePropertyIsSetTo(this.Report2, name, type, value);
        }

        [Given(@"der '(.*)'. Eintrag der '(.*)'. Aktivität von Report 1 ist auf '(.*)' gesetzt")]
        public void GivenReport1TheEntryTypeIsSetTo(int activityIndex, int entryIndex, string value)
        {
            this.GivenTheEntryTypeIsSetTo(this.Report1, activityIndex, entryIndex, value);
        }

        [Given(@"der '(.*)'. Eintrag der '(.*)'. Aktivität von Report 2 ist auf '(.*)' gesetzt")]
        public void GivenReport2TheEntryTypeIsSetTo(int activityIndex, int entryIndex, string value)
        {
            this.GivenTheEntryTypeIsSetTo(this.Report2, activityIndex, entryIndex, value);
        }

        [Then(@"enthält das Ergebnis '(.*)' Objekte\(e\)")]
        public void ThenTheResultContainsNrOfElements(int nrOfResults)
        {
            var diffResult = this.Report1.DiffList(this.Report2);

            Assert.Equal(nrOfResults, diffResult.Count);
        }

        [Then(@"ein Element besitzt den Status '(.*?)' mit der Id '(.*?)', dem Wert1 '(.*?)' und dem Wert2 '(.*?)'")]
        public void ThenTheResultDoesNotContainsEntry(Difference difference, DifferenceIdType differenceId, string value1, string value2)
        {
            if (string.IsNullOrWhiteSpace(value1))
                value1 = null;

            if (string.IsNullOrWhiteSpace(value2))
                value2 = null;

            var diffResults = this.Report1.DiffList(this.Report2);

            var filteredDiffResults = diffResults.Where(x => x.Difference == difference &&
                                                             x.DifferenceId == differenceId);

            DiffObject diffResult = null;

            foreach (var dr in filteredDiffResults)
            {
                var value1AString = dr.Value1 is double doubleValue1
                    ? doubleValue1.ToString(CultureInfo.InvariantCulture)
                    : dr.Value1?.ToString();

                var value2AString = dr.Value2 is double doubleValue2
                    ? doubleValue2.ToString(CultureInfo.InvariantCulture)
                    : dr.Value2?.ToString();

                if (value1 == value1AString && value2 == value2AString)
                {
                    diffResult = dr;
                    break;
                }
            }

            Assert.NotNull(diffResult);

            //var value1AString = "";
            //if (diffResult.Value1 is double doubleValue1)
            //    value1AString = doubleValue1.ToString(CultureInfo.InvariantCulture);
            //else
            //    value1AString = diffResult.Value1?.ToString();

            //var value1AString = diffResult.Value1 is double doubleValue1
            //    ? doubleValue1.ToString(CultureInfo.InvariantCulture)
            //    : diffResult.Value1?.ToString();

            //var value2AString = "";
            //if (diffResult.Value2 is double doubleValue2)
            //    value2AString = doubleValue2.ToString(CultureInfo.InvariantCulture);
            //else
            //    value2AString = diffResult.Value2?.ToString();

            //Assert.Equal(difference, diffResult.Difference);
            //Assert.Equal(differenceId, diffResult.DifferenceId);
            //Assert.Equal(value1, value1AString);
            //Assert.Equal(value2, value2AString);
        }

        private void GivenReportAddPersons(HkpvReport report, string values)
        {
            var ids = values?.Split(',');

            foreach (var strId in ids)
            {
                int id = 0;

                int.TryParse(strId, out id);

                var person = HkpvDataGenerator.Instance.CreatePerson(id);
                report.Persons.Add(person);

                var activity = new Activity();
                activity.PersonId = id.ToString();

                activity.Entries.Add(new ActivityType());

                report.Activities.Add(activity);
            }
        }

        private void GivenReportAddPerson(HkpvReport report, string strId, int nrOfClientActivities)
        {
            int.TryParse(strId, out int id);

            var person = HkpvDataGenerator.Instance.CreatePerson(id);
            report.Persons.Add(person);

            for (int i = 0; i < nrOfClientActivities; i++)
            {
                var activity = new Activity();
                activity.PersonId = id.ToString();

                activity.Entries.Add(ActivityType.Lv01);

                report.Activities.Add(activity);
            }
        }

        private void GivenReportAddStaff(HkpvReport report, string strId, int nrOfStaffActivities, int nrOfEmployments, float employment)
        {
            int id = 0;

            int.TryParse(strId, out id);

            var staff = HkpvDataGenerator.Instance.CreateStaff(report, id, nrOfEmployments,  employment);
            report.Staffs.Add(staff);

            for (var i = 0; i < nrOfStaffActivities; i++)
            {
                var activity = new Activity();
                activity.StaffId = id.ToString();

                activity.Entries.Add(ActivityType.Lv01);

                report.Activities.Add(activity);

            }
        }

        private void GivenThePropertyIsSetTo(HkpvReport report, string name, string type, string value)
        {
            if (type == nameof(HkpvReport))
                report.SetValue(name, value);
            else if (type == nameof(Person))
                report.Persons[0].SetValue(name, value);
            else if (type == nameof(Staff))
                report.Staffs[0].SetValue(name, value);
            else if (type == nameof(Employment))
                report.Staffs[0].Employments[0].SetValue(name, value);
            else if (type == nameof(Activity) && name == "entries")
                foreach (var a in report.Activities)

                    for (int i = 0; i < a.Entries.Count; i++)
                    {
                        if (System.Enum.TryParse(value, out ActivityType c))
                        {
                            a.Entries[i] = c;
                        }
                    }



            else if (type == nameof(Activity))
                foreach (var a in report.Activities)
                    a.SetValue(name, value);

            else
                throw new NotImplementedException();
        }

        private void GivenTheEntryTypeIsSetTo(HkpvReport report, int activtyIndex, int entryIndex, string value)
        {
            if (System.Enum.TryParse(value, out ActivityType c))
            {
                report.Activities[activtyIndex - 1].Entries[entryIndex - 1] = c;
            }
        }
    }

}
