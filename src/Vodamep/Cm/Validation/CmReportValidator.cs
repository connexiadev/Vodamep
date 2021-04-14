﻿using FluentValidation;
using FluentValidation.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodamep.Cm.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Cm.Validation
{

    internal class CmReportValidator : AbstractValidator<CmReport>
    {
        static CmReportValidator()
        {
            var isGerman = Thread.CurrentThread.CurrentCulture.Name.StartsWith("de", StringComparison.CurrentCultureIgnoreCase);
            if (isGerman)
            {
                var loc = new DisplayNameResolver();
                ValidatorOptions.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
            }
        }
        public CmReportValidator()
        {
            this.RuleFor(x => x.Institution).NotEmpty();
            
            this.RuleFor(x => x).SetValidator(new ReportDateValidator());

            this.RuleFor(x => x).SetValidator(new UniqePersonValidator());

            var earliestBirthday = new DateTime(1890, 01, 01);
            var nameRegex = "^[a-zA-ZäöüÄÖÜß][-a-zA-ZäöüÄÖÜß ]*?[a-zA-ZäöüÄÖÜß]$";
            this.RuleForEach(report => report.Persons).SetValidator(new PersonBirthdayValidator(earliestBirthday));
            this.RuleForEach(report => report.Persons).SetValidator(new PersonNameValidator(nameRegex, 2, 30, 2, 50));
            this.RuleForEach(report => report.Persons).SetValidator(new CmPersonValidator());

            this.RuleForEach(report => report.Activities).SetValidator(r => new CmActivityValidator(r));
            this.RuleForEach(report => report.ClientActivities).SetValidator(r => new CmClientActivityValidator(r));
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<CmReport> context, CancellationToken cancellation = default(CancellationToken))
        {
            return new CmReportValidationResult(await base.ValidateAsync(context, cancellation));
        }

        public override ValidationResult Validate(ValidationContext<CmReport> context)
        {
            return new CmReportValidationResult(base.Validate(context));
        }
    }
}
