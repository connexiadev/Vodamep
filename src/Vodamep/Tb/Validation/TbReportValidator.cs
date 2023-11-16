using FluentValidation;
using FluentValidation.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodamep.Tb.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Tb.Validation
{

    internal class TbReportValidator : AbstractValidator<TbReport>
    {
        private static readonly DisplayNameResolver displayNameResolver;

        static TbReportValidator()
        {
            CultureCheck.Check();

            displayNameResolver = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => displayNameResolver.GetDisplayName(memberInfo?.Name);
        }


        public TbReportValidator()
        {
            this.RuleFor(x => x.Institution).NotEmpty();

            this.RuleFor(x => x).SetValidator(new ReportDateValidator());

            var earliestBirthday = new DateTime(1890, 01, 01);
            var nameRegex = "^[a-zA-ZäöüÄÖÜß][-a-zA-ZäöüÄÖÜß ]*?[a-zA-ZäöüÄÖÜß]$";
            this.RuleForEach(report => report.Persons).SetValidator(new PersonBirthdayValidator(earliestBirthday, displayNameResolver.GetDisplayName(nameof(Person))));
            this.RuleForEach(report => report.Persons).SetValidator(new PersonNameValidator(displayNameResolver.GetDisplayName(nameof(Person)), nameRegex, 2, 30, 2, 50));
            this.RuleForEach(report => report.Persons).SetValidator(x => new TbPersonValidator(x.FromD));
            this.RuleForEach(report => report.Persons).SetValidator(x => new UniqePersonValidatorWithClientId(x.Persons));
            this.RuleForEach(report => report.Persons).SetValidator(x => new PersonHasOnlyOneActivtyValidator(x.Activities));

            this.RuleForEach(report => report.Activities).SetValidator(x => new PersonActivityHasValidPersonValidator(x.Persons));
            this.RuleForEach(report => report.Activities).SetValidator(new TbActivityValidator());
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<TbReport> context, CancellationToken cancellation = default(CancellationToken))
        {
            return new TbReportValidationResult(await base.ValidateAsync(context, cancellation));
        }

        public override ValidationResult Validate(ValidationContext<TbReport> context)
        {
            return new TbReportValidationResult(base.Validate(context));
        }
    }
}
