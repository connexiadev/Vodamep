﻿using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vodamep.Cm.Model;

namespace Vodamep.Cm.Validation
{
    public class CmReportValidationResultListFormatter : CmReportValidationResultFormatterBase
    {

        public CmReportValidationResultListFormatter(ResultFormatterTemplate template, bool ignoreWarnings = false)
        : base(template, ignoreWarnings)
        {
        }

        public IEnumerable<string> Format(CmReport report, ValidationResult validationResult)
        {
            var result = new List<string>();

            var severities = validationResult.Errors
                .Where(x => !_ignoreWarnings || x.Severity == FluentValidation.Severity.Error)
                .OrderBy(x => x.Severity);

            foreach (var severity in severities)
            {
                string message = "";

                string info = this.GetInfo(report, severity.PropertyName);
                message += info;

                if (!String.IsNullOrWhiteSpace(info))
                    message += " - ";

                message += severity.ErrorMessage;


                string value = "";
                if (severity.AttemptedValue?.GetType() == typeof(DateTime))
                {
                    DateTime dateTime = (DateTime)severity.AttemptedValue;
                    value += dateTime.ToShortDateString();
                }
                else
                {
                    value = severity.AttemptedValue?.ToString();
                }


                if (!String.IsNullOrWhiteSpace(value))
                {
                    message += " - ";
                    message += value;
                }

                result.Add(message);
            }

            return result;
        }

    }
}
