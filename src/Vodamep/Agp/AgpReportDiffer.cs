﻿using System.Collections.Generic;
using Vodamep.Agp.Model;
using Vodamep.ReportBase;

namespace Vodamep.Agp
{
    internal class AgpReportDiffer
    {
        public List<DiffObject> DiffList(AgpReport report1, AgpReport report2)
        {
            var result = new List<DiffObject>();

            //result.Add(this.FindChangedActivity(report1, report2));
            //result.Add(this.DiffStaffActivity(report1, report2));
            //result.Add(this.DiffPersonActivity(report1, report2));
            //result.Add(this.DiffEmployments(report1, report2));

            //result.AddRange(this.FindAddPersons(report1, report2));
            //result.AddRange(this.FindChangedPersons(report1, report2));
            //result.AddRange(this.FindMissingPersons(report1, report2));

            //result.AddRange(this.FindAddStaff(report1, report2));
            //result.AddRange(this.FindChangedStaff(report1, report2));
            //result.AddRange(this.FindMissingStaffs(report1, report2));

            //result.RemoveAll(x => x.Difference == Difference.Unchanged);

            return result;
        }
    }
}