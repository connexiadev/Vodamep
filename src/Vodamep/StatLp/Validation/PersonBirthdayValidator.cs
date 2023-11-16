﻿using FluentValidation;
using System;
using Google.Protobuf.WellKnownTypes;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class PersonBirthdayValidator : AbstractValidator<Person>
    {
        public PersonBirthdayValidator()
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Geburtsdatum

            // CheckDef: Erlaubte Werte
            // Fields: Geburtsdatum, Remark: > 01.01.1890, nicht in der Zukunft

            #endregion

            this.RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Birthday)
                .NotEmpty();

            RuleFor(x => x.Birthday)
                .SetValidator(new TimestampWithOutTimeValidator<Person,Timestamp>());

            RuleFor(x => x.BirthdayD)
                .LessThan(DateTime.Today)
                .Unless(x => x.Birthday == null)
                .WithMessage(Validationmessages.BirthdayNotInFuture);

            RuleFor(x => x.BirthdayD)
               .GreaterThanOrEqualTo(new DateTime(1890, 01, 01))
               .Unless(x => x.Birthday == null);
        }
    }
}
