using FluentValidation;
using FluentValidation.Validators;
using Google.Protobuf.WellKnownTypes;

namespace Vodamep.ValidationBase
{
    internal class TimestampWithOutTimeValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        public override string Name => nameof(TimestampWithOutTimeValidator<T, TProperty>);

        private readonly string messageTemplate;

        public TimestampWithOutTimeValidator()
        {
            this.messageTemplate = Validationmessages.DateMustnotHaveTime;
        }

        public TimestampWithOutTimeValidator(string propertyName, string client)
        {
            this.messageTemplate = Validationmessages.ReportBaseDateMustNotHaveTime(propertyName, client);
        }

        //public TimestampWithOutTimeValidator()
        //    : base(Validationmessages.DateMustnotHaveTime)
        //{

        //}

        //public TimestampWithOutTimeValidator(string propertyName, string client)
        //    : base(Validationmessages.ReportBaseDateMustNotHaveTime(propertyName, client))
        //{

        //}

        //protected override bool IsValid(PropertyValidatorContext context)
        //{
        //    if (!(context.PropertyValue is Timestamp ts)) return true;

        //    var date = ts.ToDateTime();

        //    return date.Date == date; 
        //}
        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            if (!(value is Timestamp ts)) return true;

            var date = ts.ToDateTime();

            return date.Date == date;
        }

        protected override string GetDefaultMessageTemplate(string errorCode) => this.messageTemplate;
    }
}
