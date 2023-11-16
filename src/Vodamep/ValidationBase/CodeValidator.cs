using FluentValidation;
using FluentValidation.Validators;
using Vodamep.Data;

namespace Vodamep.ValidationBase
{
    internal class CodeValidator<T,TProperty, TCode> : PropertyValidator<T, TProperty>
        where TCode : CodeProviderBase
    {

        public override string Name => nameof(CodeValidator<T, TProperty, TCode>);

        protected override string GetDefaultMessageTemplate(string errorCode) => Validationmessages.InvalidCode;

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            var code = value as string;

            if (string.IsNullOrEmpty(code)) return true;

            var provider = CodeProviderBase.GetInstance<TCode>();

            bool isValid = provider.IsValid(code);

            return isValid;
        }
    }
}
