using FluentValidation;
using FluentValidation.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Vodamep.Agp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Agp.Validation
{

    internal class AgpReportValidator : AbstractValidator<AgpReport>
    {
        private static readonly AgpDisplayNameResolver displayNameResolver;
        static AgpReportValidator()
        {
            CultureCheck.Check();

            displayNameResolver = new AgpDisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => displayNameResolver.GetDisplayName(memberInfo?.Name);
        }

        public AgpReportValidator()
        {
            this.RuleFor(x => x.Institution).NotEmpty();
            this.RuleFor(x => x.Institution).SetValidator(new InstitutionValidator());
            this.RuleFor(x => x.From).NotEmpty();
            this.RuleFor(x => x.To).NotEmpty();
            this.RuleFor(x => x.From).SetValidator(new TimestampWithOutTimeValidator<AgpReport, Timestamp>());
            this.RuleFor(x => x.To).SetValidator(new TimestampWithOutTimeValidator<AgpReport, Timestamp>());
            this.RuleFor(x => x.ToD).LessThanOrEqualTo(x => DateTime.Today);
            this.RuleFor(x => x.ToD).GreaterThan(x => x.FromD).Unless(x => x.From == null || x.To == null);

            ////corert kann derzeit nicht mit AnonymousType umgehen. Vielleicht später: this.RuleFor(x => new { x.From, x.To })
            this.RuleFor(x => new Tuple<DateTime, DateTime>(x.FromD, x.ToD))
                .Must(x => x.Item2 == x.Item1.LastDateInMonth())
                .Unless(x => x.From == null || x.To == null)
                .WithMessage(Validationmessages.OneMonth);

            this.RuleFor(x => x.ToD)
                .Must(x => x == x.LastDateInMonth())
                .Unless(x => x.To == null)
                .WithMessage(Validationmessages.LastDateInMonth);

            this.RuleFor(x => x.FromD)
                .Must(x => x.Day == 1)
                .Unless(x => x.From == null)
                .WithMessage(Validationmessages.FirstDateInMonth);

            this.RuleForEach(report => report.Persons).SetValidator(new AgpPersonValidator());
            this.RuleForEach(report => report.Persons).SetValidator(new PersonNameValidator(displayNameResolver.GetDisplayName(nameof(Person)), @"^[\p{L}][-\p{L}. ]*[\p{L}.]$", -1, -1, -1, -1));
            this.RuleForEach(report => report.Persons).SetValidator(new PersonBirthdayValidator(new DateTime(1900, 01, 01), displayNameResolver.GetDisplayName(nameof(Person))));

            this.RuleForEach(report => report.Activities).SetValidator(r => new ActivityValidator(r));
            this.RuleFor(report => report).SetValidator(r => new ActivityTypeValidator());

            this.RuleForEach(report => report.StaffActivities).SetValidator(r => new StaffActivityValidator(r));

            this.Include(new AgpReportPersonIdValidator());
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<AgpReport> context, CancellationToken cancellation = default(CancellationToken))
        {
            return new AgpReportValidationResult(await base.ValidateAsync(context, cancellation));
        }

        public override ValidationResult Validate(ValidationContext<AgpReport> context)
        {
            return new AgpReportValidationResult(base.Validate(context));
        }
    }
}
