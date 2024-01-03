using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vodamep.Agp.Model;

namespace Vodamep.Data.Dummy
{
    internal class AgpDataGenerator : GeneratorBase
    {
        private static AgpDataGenerator _instance;

        public static AgpDataGenerator Instance
        {
            get
            {

                if (_instance == null)
                    _instance = new AgpDataGenerator();

                return _instance;
            }
        }


        private AgpDataGenerator()
        {

        }


        public AgpReport CreateAgpReport(string institutionId = "", int? year = null, int? month = null, int persons = 100, int staffs = 5, bool useRandomValues = true, bool addActivities = true)
        {
            var report = new AgpReport()
            {

                Institution = new Institution() { Id = string.IsNullOrEmpty(institutionId) ? "agp_test" : institutionId, Name = "Testverein" }
            };

            var from = year.HasValue || month.HasValue ? new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1) : DateTime.Today.FirstDateInMonth().AddMonths(-1);


            report.FromD = from;
            report.ToD = report.FromD.LastDateInMonth();

            report.AddDummyPersons(persons);

            if (addActivities)
            {
                report.AddDummyActivities();
                report.AddDummyStaffActivities();
            }

            return report;
        }


        public Person CreatePerson(string id = null)
        {
            id = id ?? (_id++).ToString();

            var person = new Person()
            {
                Id = id,
                FamilyName = id == null ? _familynames[_rand.Next(_familynames.Length)] : _familynames[0],
                GivenName = id == null ? _names[_rand.Next(_names.Length)] : _names[0],
                Insurance = "19",

                CareAllowance = ((CareAllowance[])(Enum.GetValues(typeof(CareAllowance))))
                            .Where(x => x != CareAllowance.Any && x != CareAllowance.UndefinedAllowance)
                            .ElementAt(_rand.Next(Enum.GetValues(typeof(Referrer)).Length - 2)),

                Gender = _rand.Next(2) == 1 ? Gender.FemaleGe : Gender.MaleGe,

                Referrer = ((Referrer[])(Enum.GetValues(typeof(Referrer))))
                            .Where(x => x != Referrer.OtherReferrer &&
                                        x != Referrer.UndefinedReferrer)
                            .ElementAt(_rand.Next(Enum.GetValues(typeof(Referrer)).Length - 2)),

                Nationality = "AT",
            };

            person.Diagnoses.Add(DiagnosisGroup.AffectiveDisorder);

            // die Anschrift
            {
                var address = _addresses[_rand.Next(_addresses.Length)].Split(';');

                person.Postcode = "6800";
                person.City = "Feldkirch";
            }

            person.BirthdayD = new DateTime(1920, 01, 01).AddDays(_rand.Next(20000));

            return person;

        }

        public Person CreatePerson(int index)
        {
            var person = new Person()
            {
                Id = index.ToString(),
                Insurance = "19",

                CareAllowance = ((CareAllowance[])(Enum.GetValues(typeof(CareAllowance))))
                            .Where(x => x != CareAllowance.Any)
                            .ElementAt(index),

                Gender = _rand.Next(2) == 1 ? Gender.FemaleGe : Gender.MaleGe,
            };

            person.Diagnoses.Add(DiagnosisGroup.AffectiveDisorder);


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
                yield return CreatePerson((i + 1).ToString());
        }

        private ActivityType[] CreateRandomActivities()
        {
            var a = _activities[_rand.Next(_activities.Length)];

            if (string.IsNullOrEmpty(a)) return new ActivityType[0];

            return a.Split(',').Select(x => (ActivityType)int.Parse(x)).Distinct().ToArray();
        }


        public Activity[] CreateActivities(AgpReport report)
        {
            var result = new List<Activity>();

            foreach (var person in report.Persons)
            {
                var count = _rand.Next(1, 10); // bis max. 10 Leistungen pro Monat
                var date = report.FromD.AddDays(_rand.Next(report.ToD.Day - report.FromD.Day + 1));

                while(count > 0)
                {
                    // Pro Tag und Person nur ein Eintrag erlaubt
                    if (!result.Where(x => x.PersonId == person.Id && x.DateD.Equals(date)).Any())
                    {
                        var a = CreateRandomActivity(person.Id, date);

                        result.Add(a);
                    }

                    count--;
                }
            }

            return result.ToArray();
        }

        private Activity CreateRandomActivity(string personId, DateTime date)
        {
            var result = new Activity()
            {
                Id = System.Guid.NewGuid().ToString(),
                PersonId = personId,
                DateD = date,
                PlaceOfAction = ((PlaceOfAction[])(Enum.GetValues(typeof(PlaceOfAction))))
                .Where(x => x != PlaceOfAction.UndefinedPlace)
                .ElementAt(_rand.Next(Enum.GetValues(typeof(PlaceOfAction)).Length - 1))
            };

            var minutes = _rand.Next(1, 60) * 5; // max. 5 Stunden und in 5 Min. Schritten

            result.Minutes = minutes;

            var activities = CreateRandomActivities();
            result.Entries.AddRange(activities.OrderBy(x => x));

            return result;
        }



        public StaffActivity[] CreateStaffActivities(AgpReport report)
        {
            var result = new List<StaffActivity>();

            var count = _rand.Next(1, 50); // bis max. 50 Leistungen pro Monat
            while (count > 0)
            {
                var day = _rand.Next(1, DateTime.DaysInMonth(report.FromD.Year, report.FromD.Month));  // irgenein Datum im aktuellen Berichtszeitraum
                var date = report.FromD.AddDays(day);
                var minutes = _rand.Next(1, 60) * 5;       // irgendwas bis max. 5 Std. in 5 Min.-Schritten

                result.Add(CreateStaffActivity(date, minutes));

                count--;
            }

            return result.ToArray();
        }


        private StaffActivity CreateStaffActivity(DateTime date, int minutes)
        {
            var result = new StaffActivity()
            {
                Id = System.Guid.NewGuid().ToString(),
                DateD = date,
                ActivityType = StaffActivityType.NetworkingSa,
                Minutes = minutes,
            };

            return result;
        }

    }
}
