using FluentValidation;
using FluentValidation.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class StatLpReportValidator : AbstractValidator<StatLpReport>
    {
        static StatLpReportValidator()
        {
            CultureCheck.Check();

            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
        }


        public StatLpReportValidator()
        {

            this.RuleFor(x => x.Institution).NotEmpty();
            this.RuleFor(x => x.Institution).SetValidator(new InstitutionValidator());
            this.RuleFor(x => x.From).NotEmpty();
            this.RuleFor(x => x.To).NotEmpty();
            this.RuleFor(x => x.From).SetValidator(new TimestampWithOutTimeValidator<StatLpReport, Timestamp>());
            this.RuleFor(x => x.To).SetValidator(new TimestampWithOutTimeValidator<StatLpReport, Timestamp>());
            this.RuleFor(x => x.ToD).LessThanOrEqualTo(x => DateTime.Today);
            this.RuleFor(x => x.ToD).GreaterThan(x => x.FromD).Unless(x => x.From == null || x.To == null);

            this.RuleForEach(report => report.Persons).SetValidator(new PersonValidator());

            this.RuleFor(report => report).SetValidator(new FindDoubletsValidator());

            this.RuleFor(x => x).SetValidator(new UniqePersonIdValidator());

            this.RuleFor(x => x).SetValidator(new PersonHasUniqueIdValidator());


            this.RuleFor(x => new { x.FromD, x.ToD })
                .Must(x => x.FromD.Year == x.ToD.Year)
                .Unless(x => x.From == null || x.To == null)
                .WithMessage(Validationmessages.SameYear);

            this.RuleFor(x => x.ToD)
                .Must(x => x == x.LastDateInMonth())
                .Unless(x => x.To == null)
                .WithMessage(Validationmessages.LastDateInMonth);

            //ausgeklammert, weil bis zu jedem beliebigen Monatsende gesendet werden können soll
            //nur so funktionieren Validierungen für begrenzte Aufenthaltsdauer bei offenem Aufenthaltsende
            //this.RuleFor(x => x.ToD)
            //    .Must(x => x.Day == 31 && x.Month == 12)
            //    .Unless(x => x.To == null)
            //    .WithMessage(Validationmessages.LastDateInYear);

            this.RuleFor(x => x.FromD)
                .Must(x => x.Day == 1 && x.Month == 1)
                .Unless(x => x.From == null)
                .WithMessage(Validationmessages.FirstDateInYear);

            this.RuleFor(x => x).SetValidator(report => new AdmissionsValidator());
            this.RuleForEach(report => report.Admissions).SetValidator(report => new AdmissionValidator(report));

            this.RuleFor(x => x).SetValidator(report => new AttributesValidator());
            this.RuleForEach(report => report.Attributes).SetValidator(report => new AttributeValidator(report));

            this.RuleFor(x => x).SetValidator(report => new LeavingsValidator());
            this.RuleForEach(report => report.Leavings).SetValidator(report => new LeavingValidator(report));

            this.RuleForEach(report => report.Stays).SetValidator(report => new StayValidator(report));
            this.RuleFor(report => report).SetValidator(new PersonStayValidator());
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<StatLpReport> context, CancellationToken cancellation = default(CancellationToken))
        {
            return new StatLpReportValidationResult(await base.ValidateAsync(context, cancellation));
        }

        public override ValidationResult Validate(ValidationContext<StatLpReport> context)
        {
            return new StatLpReportValidationResult(base.Validate(context));
        }
    }
}
