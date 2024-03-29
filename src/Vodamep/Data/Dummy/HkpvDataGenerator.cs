﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vodamep.Hkpv.Model;
using Vodamep.Hkpv.Validation;

namespace Vodamep.Data.Dummy
{
    internal class HkpvDataGenerator : GeneratorBase
    {

        private static HkpvDataGenerator _instance;

        public static HkpvDataGenerator Instance
        {
            get
            {

                if (_instance == null)
                    _instance = new HkpvDataGenerator();

                return _instance;
            }
        }


        private CareAllowance[] _careAllowances = new[] { CareAllowance.L1, CareAllowance.L2, CareAllowance.L3,
                CareAllowance.L4, CareAllowance.L5, CareAllowance.L5, CareAllowance.L7,
                CareAllowance.Any, CareAllowance.Unknown };


        private HkpvDataGenerator()
        {

        }


        public HkpvReport CreateHkpvReport(string institutionId = "", int? year = null, int? month = null, int persons = 100, int staffs = 5, bool addActivities = true)
        {
            var report = new HkpvReport()
            {
                Institution = new Institution() { Id  = string.IsNullOrWhiteSpace(institutionId) ? "kpv_test" : institutionId, Name = "Testverein" }
            };

            var from = year.HasValue || month.HasValue ? new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1) : DateTime.Today.FirstDateInMonth().AddMonths(-1);


            report.FromD = from;
            report.ToD = report.FromD.LastDateInMonth();

            report.AddDummyPersons(persons);
            report.AddDummyStaffs(staffs);

            if (addActivities)
                report.AddDummyActivities();

            return report;
        }

        public Person CreatePerson()
        {
            var id = (_id++).ToString();

            var person = new Person()
            {
                Id = id,
                FamilyName = _familynames[_rand.Next(_familynames.Length)],
                GivenName = _names[_rand.Next(_names.Length)],
                Insurance = "19",
                Nationality = "AT",
                CareAllowance = _careAllowances[_rand.Next(_careAllowances.Length)],
                Gender = _rand.Next(2) == 1 ? Gender.Female : Gender.Male
            };

            // die Anschrift
            {
                var address = _addresses[_rand.Next(_addresses.Length)].Split(';');

                person.Postcode = address[6];
                person.City = address[3];
            }

            person.BirthdayD = new DateTime(1920, 01, 01).AddDays(_rand.Next(20000));

            person.Ssn = CreateRandomSSN(person.BirthdayD);

            return person;

        }

        public Person CreatePerson(int index)
        {
            var person = new Person()
            {
                Id = index.ToString(),
                FamilyName = _familynames[index],
                GivenName = _names[index],
                Insurance = "19",
                Nationality = "AT",
                CareAllowance = _careAllowances[index],
                Gender = index % 2 == 1 ? Gender.Female : Gender.Male
            };

            // die Anschrift
            {
                var address = _addresses[index].Split(';');

                person.Postcode = address[6];
                person.City = address[3];
            }

            person.BirthdayD = new DateTime(1920, 01, 01).AddDays(index);

            person.Ssn = CreateFirstPossibleSSN(person.BirthdayD, index);

            return person;
        } 

        public IEnumerable<Person> CreatePersons(int count)
        {
            for (var i = 0; i < count; i++)
                yield return CreatePerson();
        }


        public string CreateFirstPossibleSSN(DateTime date, int index)
        {
            int nr;
            int cd;

            while (true)
            {
                nr = index + 100;

                cd = SSNHelper.GetCheckDigit(nr.ToString("000"), date.ToString("ddMMyy"));

                if (cd >= 0 && cd <= 9)
                    break;

                index++;

            }

            return SSNHelper.Format(string.Format("{0}{1}{2:ddMMyy}", nr, cd, date));
        }

        public string CreateRandomSSN(DateTime date)
        {
            int nr;
            int cd;

            while (true)
            {
                nr = _rand.Next(899) + 100;

                cd = SSNHelper.GetCheckDigit(nr.ToString("000"), date.ToString("ddMMyy"));

                if (cd >= 0 && cd <= 9)
                    break;
            }

            return SSNHelper.Format(string.Format("{0}{1}{2:ddMMyy}", nr, cd, date));
        }

        public Staff CreateStaff(HkpvReport report)
        {
            var id = (_id++).ToString();

            var staff = new Staff
            {
                Id = id,
                FamilyName = _familynames[_rand.Next(_familynames.Length)],
                GivenName = _names[_rand.Next(_names.Length)],
                Qualification = "DGKP"
            };

            staff.Employments.Add(new Employment()
            {
                HoursPerWeek = 38.5F,
                FromD = report.FromD,
                ToD = report.ToD
            });

            return staff;
        }

        public Staff CreateStaff(HkpvReport report, int index, int nrOfEmployments, float employment)
        {
            var id = index.ToString();

            var staff = new Staff
            {
                Id = id,
                FamilyName = _familynames[index],
                GivenName = _names[index],
                Qualification = "DGKP"
            };

            for (int i = 0; i < nrOfEmployments; i++)
            {
                staff.Employments.Add(new Employment()
                {
                    HoursPerWeek = employment,
                    FromD = report.FromD,
                    ToD = report.ToD
                });
            }

            return staff;
        }

        public IEnumerable<Staff> CreateStaffs(HkpvReport report, int count)
        {
            for (var i = 0; i < count; i++)
                yield return CreateStaff(report);
        }

        private ActivityType[] CreateRandomActivities()
        {
            var a = _activities[_rand.Next(_activities.Length)];

            if (string.IsNullOrEmpty(a)) return new ActivityType[0];

            return a.Split(',').Select(x => (ActivityType)int.Parse(x)).ToArray();
        }

        private Activity CreateRandomActivity(string personId, string staffId, DateTime date)
        {
            var result = new Activity()
            {
                StaffId = staffId,
                PersonId = personId,
                DateD = date                
            };

            var activities = CreateRandomActivities();
            result.Entries.AddRange(activities.OrderBy(x =>x));

            return result;
        }

        public Activity[] CreateActivities(HkpvReport report)
        {
            var result = new List<Activity>();

            foreach (var staff in report.Staffs)
            {
                // die zu betreuenden Personen zufällig zuordnen
                var persons = report.Persons.Count == 1 || report.Staffs.Count == 1 ? report.Persons.ToArray() : report.Persons.Where(x => _rand.Next(report.Staffs.Count) == 0).ToArray();

                // ein Mitarbeiter soll pro Monat max. 6000 Minuten arbeiten. 
                // wenn nur wenige Personen betreut werden: max 500 Minuten pro Person
                var minuten = _rand.Next(Math.Min(persons.Count() * 500, 6000));

                while (minuten > 0)
                {
                    var personId = persons[_rand.Next(persons.Length)].Id;

                    var date = report.FromD.AddDays(_rand.Next(report.ToD.Day - report.FromD.Day + 1));

                    // Pro Tag, Person und Mitarbeiter nur ein Eintrag erlaubt:
                    if (!result.Where(x => x.PersonId == personId && x.StaffId == staff.Id && x.DateD.Equals(date)).Any())
                    {
                        var a = CreateRandomActivity(personId, staff.Id, date);

                        minuten -= a.GetMinutes();

                        result.Add(a);
                    }
                }
            }

            //sicherstellen, dass jede Person zumindest einen Eintrag hat
            foreach (var p in report.Persons.Where(x => !result.Where(a => a.PersonId == x.Id).Any()).ToArray())
            {
                var date = report.FromD.AddDays(_rand.Next(report.ToD.Day - report.FromD.Day + 1));

                result.Add(CreateRandomActivity(p.Id, report.Staffs[_rand.Next(report.Staffs.Count)].Id, date));
            }


            return result.ToArray();
        }
    }
}
