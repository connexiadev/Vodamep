using FluentValidation;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class PersonSsnValidator : AbstractValidator<Person>
    {
        public PersonSsnValidator()   
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Hart

            // CheckDef: Erlaubte Werte
            // Detail: SVNR, Remark: Gültige SVNR, Prüfziffer, Geburtsdatum

            #endregion
            this.CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Ssn)
                .NotEmpty();

            RuleFor(x => x.Ssn)                
                .Must(x => SSNHelper.IsValid(x))
                .WithMessage(Validationmessages.SsnNotValid);            
        }

    }
}
