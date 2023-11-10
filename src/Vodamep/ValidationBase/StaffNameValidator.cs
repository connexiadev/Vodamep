using System.Text.RegularExpressions;
using FluentValidation;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{
    internal class StaffNameValidator : AbstractValidator<IStaff>
    {
        public StaffNameValidator(string propertyName, string nameRegex, int minLengthGivenName, int maxLengthGivenName, int minLengthFamilyName, int maxLengthFamilyName)
        {

            #region Documentation
            // AreaDef: MKKP
            // OrderDef: 02
            // SectionDef: Mitarbeiter
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Nachname
            // Fields: Vorname

            // CheckDef: Erlaubte Werte
            // Fields: Nachname, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            // Fields: Vorname, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            #endregion


            this.RuleFor(x => x.FamilyName).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(propertyName, x.Id));
            this.RuleFor(x => x.GivenName).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty( propertyName, x.Id));

            var r = new Regex(nameRegex);
            this.RuleFor(x => x.FamilyName)
                .Matches(r).Unless(x => string.IsNullOrEmpty(x.FamilyName))
                .WithMessage(x => Validationmessages.ReportBasePropertyInvalidFormat(propertyName, x.GetDisplayName()));

            this.RuleFor(x => x.GivenName)
                .Matches(r).Unless(x => string.IsNullOrEmpty(x.GivenName))
                .WithMessage(x => Validationmessages.ReportBasePropertyInvalidFormat(propertyName, x.GetDisplayName()));

            if (minLengthGivenName > 0)
            {
                this.RuleFor(x => x.GivenName).MinimumLength(minLengthGivenName).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.GetDisplayName()));
            }

            if (maxLengthGivenName > 0)
            {
                this.RuleFor(x => x.GivenName).MaximumLength(maxLengthGivenName).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.GetDisplayName()));
            }

            if (minLengthFamilyName > 0)
            {
                this.RuleFor(x => x.FamilyName).MinimumLength(minLengthFamilyName).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.Id));
            }

            if (maxLengthFamilyName > 0)
            {
                this.RuleFor(x => x.FamilyName).MaximumLength(maxLengthFamilyName).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.Id));
            }
        }
    }
}