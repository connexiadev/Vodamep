﻿using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Vodamep.Cm.Model;
using Vodamep.Data;
using Vodamep.ValidationBase;

namespace Vodamep.Cm.Validation
{
    internal class CmPersonValidator : AbstractValidator<Person>
    {
        public CmPersonValidator()
        {
            #region Documentation
            // AreaDef: CM
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Geschlecht
            // Fields: Pflegestufe
            // Fields: PLZ/Ort
            // Fields: Staatsbürgerschaft

            // CheckDef: Erlaubte Werte
            // Fields: Geschlecht, Remark: Geschlechter-Liste, Url: src/Vodamep/Datasets/Gender.csv
            // Fields: Pflegestufen, Remark: Pflegestufen-Liste, Url:  src/Vodamep/Datasets/CareAllowance.csv
            // Fields: PLZ/Ort, Remark: PLZ/Orte-Liste, Url: src/Vodamep/Datasets/PostcodeCity.csv
            // Fields: Staatsbürgerschaft, Remark: Staatsbürgerschaften-Liste, Url: src/Vodamep/Datasets/CountryCode.csv
            #endregion

            this.RuleFor(x => x.Gender).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.CareAllowance).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));

            this.RuleFor(x => x.Nationality).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.Nationality)
                .Must((person, country) => CountryCodeProvider.Instance.IsValid(country))
                .Unless(x => string.IsNullOrWhiteSpace(x.Nationality))
                .WithMessage(x => Validationmessages.ReportBaseInvalidValue(x.GetDisplayName()));

            this.RuleFor(x => x.Postcode).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.City).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x)
                .Must((x) =>
                {
                    if (!String.IsNullOrWhiteSpace(x.Postcode) &&
                        !String.IsNullOrWhiteSpace(x.City))
                    {
                        return PostcodeCityProvider.Instance.IsValid($"{x.Postcode} {x.City}");
                    }
                    return true;

                })
                .WithMessage(x => Validationmessages.ClientWrongPostCodeCity(x.GetDisplayName()));

        }
    }
}
