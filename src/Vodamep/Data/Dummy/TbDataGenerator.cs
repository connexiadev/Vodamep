using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Tb.Model;

namespace Vodamep.Data.Dummy
{
    internal class TbDataGenerator : GeneratorBase
    {
        private static TbDataGenerator _instance;

        public static TbDataGenerator Instance
        {
            get
            {

                if (_instance == null)
                    _instance = new TbDataGenerator();

                return _instance;
            }
        }

        private TbDataGenerator()
        {

        }


        public TbReport CreateTbReport(string institutionId = "", int? year = null, int? month = null, int persons = 100, int staffs = 5, bool addActivities = true)
        {
            var report = new TbReport()
            {
                Institution = new Institution() { Id = string.IsNullOrEmpty(institutionId) ? "tb_test" : institutionId, Name = "Testverein" }
            };

            var from = year.HasValue || month.HasValue ? new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1) : DateTime.Today.FirstDateInMonth().AddMonths(-1);


            report.FromD = from;
            report.ToD = report.FromD.LastDateInMonth();

            report.AddDummyPersons(persons);
            report.AddDummyActivities(1);

            return report;
        }



        public Person CreatePerson(int index)
        {
            var person = new Person()
            {
                Id = index.ToString(),
                FamilyName = _familynames[index],
                GivenName = _names[index],
                CareAllowance = ((CareAllowance[])(Enum.GetValues(typeof(CareAllowance))))
                    .Where(x => x != CareAllowance.UndefinedAllowance)
                    .ElementAt(_rand.Next(Enum.GetValues(typeof(CareAllowance)).Length - 1)),
                Nationality = "AT",
                AdmissionType = AdmissionType.DayCareAt,
                Gender = _rand.Next(2) == 1 ? Gender.FemaleGe : Gender.MaleGe,
                MainAttendanceRelation = MainAttendanceRelation.ChildMr,
                MainAttendanceCloseness = ((MainAttendanceCloseness[])(Enum.GetValues(typeof(MainAttendanceCloseness))))
                    .Where(x => x != MainAttendanceCloseness.UndefinedMc)
                    .ElementAt(_rand.Next(Enum.GetValues(typeof(MainAttendanceCloseness)).Length - 1)),

            };

            // die Anschrift
            {
                var address = _addresses[index].Split(';');

                person.Postcode = address[6];
                person.City = address[3];
            }

            person.BirthdayD = new DateTime(1920, 01, 01).AddDays(index);


            return person;
        }

        public IEnumerable<Person> CreatePersons(int count)
        {
            for (var i = 0; i < count; i++)
                yield return CreatePerson(i + 1);
        }

        public Activity CreateActivity(string personId)
        {
            var clientActivity = new Activity
            {
                PersonId = personId,
                HoursPerMonth = 500,
            };

            return clientActivity;
        }

        public IEnumerable<Activity> CreateActivities(TbReport report, int count)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            for (var i = 0; i < count; i++)
            {
                var personId = report.Persons[rand.Next(0, report.Persons.Count - 1)].Id;
                yield return CreateActivity(personId);
            }
        }
    }
}