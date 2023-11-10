using System;
using FluentValidation;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{

    internal class PersonBirthdayValidator : AbstractValidator<IPerson>
    {
        public PersonBirthdayValidator(DateTime earliestBirthday, string clientOrStaff)
        {
            #region Documentation
            // AreaDef: AGP
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Geburtsdatum

            // CheckDef: Erlaubte Werte
            // Detail: Geburtsdatum, Remark: > 01.01.1900, nicht in der Zukunft

            // AreaDef: CM
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Geburtsdatum

            // CheckDef: Erlaubte Werte
            // Detail: Geburtsdatum, Remark: > 01.01.1890, nicht in der Zukunft


            // AreaDef: MOHI
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Geburtsdatum

            // CheckDef: Erlaubte Werte
            // Detail: Geburtsdatum, Remark: > 01.01.1890, nicht in der Zukunft



            // AreaDef: TB
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Geburtsdatum

            // CheckDef: Erlaubte Werte
            // Detail: Geburtsdatum, Remark: > 01.01.1890, nicht in der Zukunft
            #endregion

            this.CascadeMode = CascadeMode.StopOnFirstFailure;

            this.RuleFor(x => x.BirthdayD).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));

            this.RuleFor(x => x.Birthday)
                .SetValidator(new TimestampWithOutTimeValidator()).WithMessage(x => Validationmessages.ReportBaseDateMustNotHaveTime(x.GetDisplayName(), clientOrStaff));

            RuleFor(x => x.BirthdayD)
                .LessThan(DateTime.Today)
                .Unless(x => x.Birthday == null)
                .WithMessage(x => Validationmessages.ReportBaseBirthdayNotInFuture(x.GetDisplayName(), clientOrStaff));

            RuleFor(x => x.BirthdayD)
                .GreaterThanOrEqualTo(earliestBirthday)
                .Unless(x => x.Birthday == null)
                .WithMessage(x => Validationmessages.ReportBaseBirthdayMustNotBeBefore(x.GetDisplayName(), clientOrStaff));

        }

    }
}
