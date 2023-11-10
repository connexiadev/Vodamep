using FluentValidation;
using System;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;


namespace Vodamep.Hkpv.Validation
{

    internal class PersonBirthdayValidator : AbstractValidator<Person>
    {
        public PersonBirthdayValidator()
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Geburtsdatum

            // CheckDef: Erlaubte Werte
            // Detail: Geburtsdatum, Remark: > 01.01.1900, nicht in der Zukunft
            // Detail: Geburtsdatum, Remark: Abhängigkeit zur SVNR, Strength: Warnung

            #endregion


            this.CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Birthday)
                .NotEmpty();

            RuleFor(x => x.Birthday)
                .SetValidator(new TimestampWithOutTimeValidator());

            RuleFor(x => x.BirthdayD)
                .LessThan(DateTime.Today)
                .Unless(x => x.Birthday == null)
                .WithMessage(Validationmessages.BirthdayNotInFuture);

            RuleFor(x => x.BirthdayD)
               .GreaterThanOrEqualTo(new DateTime(1900, 01, 01))
               .Unless(x => x.Birthday == null);

            RuleFor(x => x.BirthdayD)
                .Must(CheckDates)
                .Unless(x => x.Birthday == null || !SSNHelper.IsValid(x.Ssn))
                .WithSeverity(Severity.Warning)                
                .WithMessage(x => Validationmessages.BirthdayNotInSsn(x));  
        }


        private bool CheckDates(Person person, DateTime date)
        {
            if (date == null) return false;

            var date2 = SSNHelper.GetBirthDay(person.Ssn);

            if (!date2.HasValue) return true;

            return date2.Value.ToString("ddMMyy") == date.ToString("ddMMyy");
        }
    }
}
