using System;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Vodamep.ValidationBase;

namespace Vodamep.ReportBase
{
    internal class DateTimeValidator : AbstractValidator<DateTime>
    {
        public DateTimeValidator(string propertyName, string client, string staff, DateTime from, DateTime to, Timestamp timestamp)
        {
            this.RuleFor(x => x)
                .SetValidator(new TimestampWithOutTimeValidator<DateTime,DateTime>(propertyName, client))
                .Unless(x => timestamp == null);

            string personName = "";

            if (!String.IsNullOrWhiteSpace(client) && !String.IsNullOrWhiteSpace(staff))
                personName = client + " / " + staff;
            else
                personName = client + staff;

            string dateTimeText = "";
            if (timestamp != null)
            {
                DateTime date = timestamp.ToDateTime();
                dateTimeText = "am " + date.ToShortDateString();
            }
            


            if (from != DateTime.MinValue)
            {
                this.RuleFor(x => x).GreaterThanOrEqualTo(from).Unless(x => timestamp == null)
                    .WithMessage(x => Validationmessages.ReportBaseActivityDateMustBeWithinReport(propertyName, personName, dateTimeText));
            }
            if (to > from)
            {
                this.RuleFor(x => x).LessThanOrEqualTo(to).Unless(x => timestamp == null)
                    .WithMessage(x => Validationmessages.ReportBaseActivityDateMustBeWithinReport(propertyName, personName, dateTimeText));
            }
        }
    }
}