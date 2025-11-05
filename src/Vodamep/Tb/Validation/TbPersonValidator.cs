using System;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Data;
using Vodamep.Tb.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Tb.Validation
{
    internal class TbPersonValidator : AbstractValidator<Person>
    {
        public TbPersonValidator(DateTime reportDate)
        {
            #region Documentation
            // AreaDef: TB
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Geschlecht
            // Fields: PLZ/Ort
            // Fields: Pflegestufe
            // Fields: Aufnahmeart
            // Fields: Verwandtschaftsverhältnis
            // Fields: Räumliche Nähe
            // Fields: Staatsbürgerschaft

            // CheckDef: Erlaubte Werte
            // Fields: Geschlecht, Remark: Geschlechter-Liste, Url: src/Vodamep/Datasets/Gender.csv
            // Fields: PLZ/Ort, Remark: Ab 2019, PLZ/Orte-Liste, Url: src/Vodamep/Datasets/PostcodeCity.csv
            // Fields: Aufnahmearten, Remark: Aufnahmearten-Liste, Url:  src/Vodamep/Datasets/Tb/AdmissionType.csv
            // Fields: Pflegestufen, Remark: Pflegestufen-Liste, Url:  src/Vodamep/Datasets/CareAllowance.csv
            // Fields: Verwandtschaftsverhältnis, Remark: Verwandtschaftsverhältnis-Liste, Url:  src/Vodamep/Datasets/MainAttendanceRelation.csv
            // Fields: Räumliche Nähe, Remark: Räumliche-Nähe-Liste, Url:  src/Vodamep/Datasets/MainAttendanceCloseness.csv
            // Fields: Staatsbürgerschaft, Remark: Staatsbürgerschaften-Liste, Url: src/Vodamep/Datasets/CountryCode.csv
            #endregion

            this.RuleFor(x => x.Gender).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.CareAllowance).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.MainAttendanceRelation).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.MainAttendanceCloseness).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));

            this.RuleFor(x => x.AdmissionType).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));



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
                    // Erst ab 2019 wurden von allen díe Gemeinde Kennzahlen übermittelt
                    // Davor war nicht sichergestellt, dass in PLZ/Ort ein definiertes Wertepaar enthält
                    if (reportDate >= new DateTime(2019, 01, 01))
                    {
                        if (!String.IsNullOrWhiteSpace(x.Postcode) &&
                            !String.IsNullOrWhiteSpace(x.City))
                        {
                            return PostcodeCityProvider.Instance.IsValid($"{x.Postcode} {x.City}");
                        }
                    }
                    return true;
                })
                .WithMessage(x => Validationmessages.ClientWrongPostCodeCity(x.GetDisplayName()));


        }
    }
}
