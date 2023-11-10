using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Vodamep.Data;
using Vodamep.Mohi.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Mohi.Validation
{
    internal class MohiPersonValidator : AbstractValidator<Person>
    {
        public MohiPersonValidator()
        {
            #region Documentation
            // AreaDef: MOHI
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Geschlecht
            // Detail: PLZ/Ort
            // Detail: Pflegestufe
            // Detail: Verwandtschaftsverhältnis
            // Detail: Räumliche Nähe
            // Detail: Staatsbürgerschaft

            // CheckDef: Erlaubte Werte
            // Detail: Geschlecht, Remark: Geschlechter-Liste, Url: src/Vodamep/Datasets/Gender.csv
            // Detail: PLZ/Ort, Remark: PLZ/Orte-Liste, Url: src/Vodamep/Datasets/PostcodeCity.csv
            // Detail: Pflegestufen, Remark: Pflegestufen-Liste, Url:  src/Vodamep/Datasets/CareAllowance.csv
            // Detail: Verwandtschaftsverhältnis, Remark: Verwandtschaftsverhältnis-Liste, Url:  src/Vodamep/Datasets/MainAttendanceRelation.csv
            // Detail: Räumliche Nähe, Remark: Räumliche-Nähe-Liste, Url:  src/Vodamep/Datasets/MainAttendanceCloseness.csv
            // Detail: Staatsbürgerschaft, Remark: Staatsbürgerschaften-Liste, Url: src/Vodamep/Datasets/CountryCode.csv
            #endregion

            this.RuleFor(x => x.Gender).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.Postcode).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.City).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.CareAllowance).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.MainAttendanceRelation).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.MainAttendanceCloseness).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));

            this.RuleFor(x => x.Nationality).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.Nationality)
                .Must((person, country) => CountryCodeProvider.Instance.IsValid(country))
                .Unless(x => string.IsNullOrWhiteSpace(x.Nationality))
                .WithMessage(x => Validationmessages.ReportBaseInvalidValue(x.GetDisplayName()));

            this.RuleFor(x => x)
                .Must((x) =>
                {
                    bool result = true;

                    // Ort / PLZ dürfen nie leer sein
                    if (!String.IsNullOrWhiteSpace(x.Postcode) &&
                        !String.IsNullOrWhiteSpace(x.City))
                    {
                        result = PostcodeCityProvider.Instance.IsValid($"{x.Postcode} {x.City}");
                    }

                    return result;

                })
                .WithMessage(x => Validationmessages.ClientWrongPostCodeCity(x.GetDisplayName()));

        }
    }
}
