using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using System;
using Vodamep.Data;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{
    internal class CodeValidDateValidator<T,TProperty, TCode> : PropertyValidator<T, TProperty>
        where TCode : CodeProviderBase
    {

        DateTime to;

        public CodeValidDateValidator(DateTime to)
        {
            this.to = to;
        }


        public override string Name => nameof(CodeValidDateValidator<T, TProperty, TCode>);

        protected override string GetDefaultMessageTemplate(string errorCode) => Validationmessages.InvalidCode;

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            var code = value as string;

            if (string.IsNullOrEmpty(code)) return true;

            var provider = CodeProviderBase.GetInstance<TCode>();

            if (!provider.IsValid(code))
            {
                context.MessageFormatter.AppendArgument("SinceText", " kein gültiger Code");
                return false;
            }
            if (!provider.IsStillValid(code, to))
            {
                string since = provider.Values[code].ValidTo.Value.ToShortDateString();

                context.MessageFormatter.AppendArgument("SinceText", $" seit {since} kein gültiger Code mehr");
                return false;
            }


            return true;

        }
    }
}
