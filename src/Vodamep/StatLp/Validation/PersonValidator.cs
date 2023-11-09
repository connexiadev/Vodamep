using FluentValidation;
using System.Text.RegularExpressions;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Nachname
            // Fields: Vorname

            // CheckDef: Erlaubte Werte
            // Fields: Nachname, Remark: 2-50 Zeichen, Buchstaben, Bindestrich, Leerzeichen
            // Fields: Vorname, Remark: 2-30 Zeichen, Buchstaben, Bindestrich, Leerzeichen
            #endregion

            this.RuleFor(x => x.FamilyName).NotEmpty();
            this.RuleFor(x => x.GivenName).NotEmpty();

            this.RuleFor(x => x.FamilyName).MinimumLength(2).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.Id));
            this.RuleFor(x => x.GivenName).MinimumLength(2).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.Id));

            this.RuleFor(x => x.FamilyName).MaximumLength(50).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.Id));
            this.RuleFor(x => x.GivenName).MaximumLength(30).WithMessage(x => Validationmessages.ReportBaseInvalidLength(x.Id));

            var r = new Regex(@"^[a-zA-ZäöüÄÖÜß][-a-zA-ZäöüÄÖÜß ]*?[a-zA-ZäöüÄÖÜß]$");
            this.RuleFor(x => x.FamilyName).Matches(r).Unless(x => string.IsNullOrEmpty(x.FamilyName));
            this.RuleFor(x => x.GivenName).Matches(r).Unless(x => string.IsNullOrEmpty(x.GivenName));

            this.Include(new PersonBirthdayValidator());
        }
    }
}
