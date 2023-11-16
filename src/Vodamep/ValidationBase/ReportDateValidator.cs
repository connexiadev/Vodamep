using System;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{
    internal class ReportDateValidator : AbstractValidator<IReport>
    {
        public ReportDateValidator()
        {
            this.RuleFor(x => x.From).NotEmpty();
            this.RuleFor(x => x.To).NotEmpty();

            this.RuleFor(x => x.From).SetValidator(new TimestampWithOutTimeValidator<IReport, Timestamp>());
            this.RuleFor(x => x.To).SetValidator(new TimestampWithOutTimeValidator<IReport, Timestamp>());

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

            this.RuleFor(x => x.ToD).LessThanOrEqualTo(x => DateTime.Today);
            this.RuleFor(x => x.ToD).GreaterThan(x => x.FromD).Unless(x => x.From == null || x.To == null);

        }
    }
}