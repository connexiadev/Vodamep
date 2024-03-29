﻿using System;
using System.Linq;
using Vodamep.Hkpv.Model;


namespace Vodamep.Data.Dummy
{
    internal static class DataGeneratorHkpvReportExtensions
    {
        public static Person AddDummyPerson(this HkpvReport report)
        {
            var p = HkpvDataGenerator.Instance.CreatePerson();
            report.AddPerson(p);
            return p;
        }

        public static Person[] AddDummyPersons(this HkpvReport report, int count)
        {
            var p = HkpvDataGenerator.Instance.CreatePersons(count).ToArray();
            report.AddPersons(p);
            return p;
        }

        public static Staff AddDummyStaff(this HkpvReport report)
        {
            var s = HkpvDataGenerator.Instance.CreateStaff(report);

            report.Staffs.Add(s);
            return s;
        }

        public static Staff[] AddDummyStaffs(this HkpvReport report, int count)
        {
            var s = HkpvDataGenerator.Instance.CreateStaffs(report, count).ToArray();
            report.Staffs.AddRange(s);
            return s;
        }

        public static Activity AddDummyActivity(this HkpvReport report, string code, DateTime? date = null)
        {
            if (!report.Persons.Any())
                report.AddDummyPerson();

            if (!report.Staffs.Any())
                report.AddDummyStaff();

            var a = new Activity()
            {
                PersonId = report.Persons[0].Id,
                StaffId = report.Staffs[0].Id,
                DateD = date ?? report.FromD
            };

            a.Entries.AddRange(code.Split(',').Select(x => (ActivityType)int.Parse(x)).OrderBy(x => x));

            report.Activities.Add(a);

            return a;
        }

        public static Activity AddDummyConsultation(this HkpvReport report, string code, DateTime? date = null)
        {
            if (!report.Staffs.Any())
                report.AddDummyStaff();

            var a = new Activity()
            {
                StaffId = report.Staffs[0].Id,
                DateD = date ?? report.FromD
            };

            a.Entries.AddRange(code.Split(',').Select(x => (ActivityType)int.Parse(x)).OrderBy(x => x));


            return a;
        }

        public static Activity[] AddDummyActivities(this HkpvReport report)
        {
            if (report.FromD == null)
                report.FromD = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);

            if (report.ToD == null)
                report.ToD = report.FromD.LastDateInMonth();

            if (report.Staffs.Count == 0)
                report.AddDummyStaff();

            if (report.Persons.Count == 0)
                report.AddDummyPerson();

            var a = HkpvDataGenerator.Instance.CreateActivities(report);
            report.Activities.AddRange(a);
            return a;
        }

    }
}
