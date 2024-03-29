﻿using FluentValidation.Results;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vodamep.Hkpv.Model;

namespace Vodamep.Hkpv.Validation
{
    public class HkpvReportValidationResultFormatter
    {

        private readonly ResultFormatterTemplate _template;
        private readonly bool _ignoreWarnings;

        public HkpvReportValidationResultFormatter(ResultFormatterTemplate template, bool ignoreWarnings = false)
        {
            _template = template;
            _ignoreWarnings = ignoreWarnings;

            _strategies = new[]
            {
                new GetNameByPatternStrategy(GetIdPattern(nameof(HkpvReport.Persons)), GetNameOfPerson),
                new GetNameByPatternStrategy(GetIdPattern(nameof(HkpvReport.Staffs)), GetNameOfStaff),
                new GetNameByPatternStrategy(GetIdPattern(nameof(HkpvReport.Activities)), GetNameOfActivity),

                new GetNameByPatternStrategy($"^{nameof(HkpvReport.To)}$",(a,b) => string.Empty),
                new GetNameByPatternStrategy($"^{nameof(HkpvReport.ToD)}$",(a,b) => string.Empty),
                new GetNameByPatternStrategy($"^{nameof(HkpvReport.From)}$",(a,b) => string.Empty),
                new GetNameByPatternStrategy($"^{nameof(HkpvReport.FromD)}$",(a,b) => string.Empty),
            };

        }

        public string Format(HkpvReport report, ValidationResult validationResult)
        {
            

            var result = new StringBuilder();

            result.Append(_template.Header(report, validationResult));

            var severities = validationResult.Errors
                .Where(x => !_ignoreWarnings || x.Severity == FluentValidation.Severity.Error)
                .OrderBy(x => x.Severity)
                .GroupBy(x => x.Severity);

            foreach (var severity in severities)
            {
                result.Append(_template.HeaderSeverity(GetSeverityName(severity.Key)));

                var entries = severity.Select(x => new
                {
                    Info = this.GetInfo(report, x.PropertyName),
                    Message = x.ErrorMessage,
                    Value = x.AttemptedValue?.ToString()
                }).ToArray();

                foreach (var groupedInfos in entries.OrderBy(x => x.Info).GroupBy(x => x.Info))
                {
                    result.Append(_template.FirstLine((groupedInfos.Key, groupedInfos.First().Message, groupedInfos.First().Value)));

                    foreach (var info in groupedInfos.Skip(1))
                    {
                        result.Append(_template.Line((info.Message, info.Value)));
                    }
                }

                result.Append(_template.FooterSeverity(severity.ToString()));
            }
            return result.ToString();
        }

        private static string GetIdPattern(string propertyName) => $@"{propertyName}\[(?<id>\d+)\]";


        private string GetSeverityName(FluentValidation.Severity severity)
        {
            switch (severity)
            {
                case FluentValidation.Severity.Error:
                    return "Fehler";
                case FluentValidation.Severity.Warning:
                    return "Warnung";
                case FluentValidation.Severity.Info:
                    return "Information";
                default:
                    return severity.ToString();
            }
        }

        private readonly GetNameByPatternStrategy[] _strategies;

        private string GetNameOfPerson(HkpvReport report, int index)
        {
            if (report.Persons.Count > index && index >= 0)
            {
                var e = report.Persons[index];
                return $"Person: {e.FamilyName} {e.GivenName}";
            }

            return string.Empty;
        }
        private string GetNameOfStaff(HkpvReport report, int index)
        {
            if (report.Staffs.Count > index && index >= 0)
            {
                var e = report.Staffs[index];
                return $"Mitarbeiter: {e.FamilyName} {e.GivenName}";
            }

            return string.Empty;
        }

        private string GetNameOfActivity(HkpvReport report, int index)
        {
            if (report.Activities.Count > index && index >= 0)
            {
                var e = report.Activities[index];
                return $"Aktivität {e.DateD:dd.MM.yyyy}{_template.Linefeed}  {String.Join(",", e.Entries)}{_template.Linefeed}  {GetNameOfPersonById(report, e.PersonId)}{_template.Linefeed}  {GetNameOfStaffById(report, e.StaffId)}";
            }

            return string.Empty;
        }

        private string GetNameOfPersonById(HkpvReport report, string id)
        {
            var e = report.Persons.Where(x => x.Id == id).FirstOrDefault();

            if (e == null)
                return string.Empty;

            return $"{e.FamilyName} {e.GivenName}";
        }

        private string GetNameOfStaffById(HkpvReport report, string id)
        {
            var e = report.Staffs.Where(x => x.Id == id).FirstOrDefault();

            if (e == null)
                return string.Empty;

            return $"{e.FamilyName} {e.GivenName}";
        }

        private string GetInfo(HkpvReport report, string propertyName)
        {
            foreach (var strategy in _strategies)
            {
                var (Success, Info) = strategy.GetInfo(report, propertyName);

                if (Success) return Info;
            }

            return propertyName;
        }

        private class GetNameByPatternStrategy
        {
            private readonly Regex _pattern;
            private readonly Func<HkpvReport, int, string> _resolveInfo;

            public GetNameByPatternStrategy(string pattern, Func<HkpvReport, int, string> resolveInfo)
            {
                _pattern = new Regex(pattern);
                _resolveInfo = resolveInfo;
            }

            public (bool Success, string Info) GetInfo(HkpvReport report, string propertyName)
            {
                var m = this._pattern.Match(propertyName);

                if (m.Success)
                {
                    int.TryParse(m.Groups["id"].Value, out int id);

                    return (true, _resolveInfo(report, id));
                }
                return (false, string.Empty);
            }
        }
    }
}
