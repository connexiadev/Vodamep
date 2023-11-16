using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Text.RegularExpressions;
using Vodamep.Data;
using Vodamep.Data.Hkpv;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Nachname
            // Fields: Vorname
            // Fields: Versicherung
            // Fields: Staatsbürgerschaft
            // Fields: Pflegestufe
            // Fields: PLZ/Ort
            // Fields: Geschlecht, Remark: Geschlechter-Liste, Url: src/Vodamep/Datasets/Gender.csv

            // CheckDef: Erlaubte Werte
            // Fields: Nachname, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            // Fields: Vorname, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            // Fields: Versicherung, Remark: Versicherungs-Liste, Url: src/Vodamep/Datasets/InsuranceCode.csv
            // Fields: Staatsbürgerschaft, Remark: Staatsbürgerschaften-Liste, Url: src/Vodamep/Datasets/CountryCode.csv
            // Fields: Pflegestufen, Remark: Pflegestufen-Liste, Url:  src/Vodamep/Datasets/CareAllowance.csv
            // Fields: PLZ/Ort, Remark: PLZ/Orte-Liste, Url: src/Vodamep/Datasets/PostcodeCity.csv
            // Fields: Geschlecht, Remark: Geschlechter-Liste, Url: src/Vodamep/Datasets/Gender.csv

            #endregion

            this.RuleFor(x => x.FamilyName).NotEmpty();
            this.RuleFor(x => x.GivenName).NotEmpty();

            // Änderung 5.11.2018, LH
            var r = new Regex(@"^[\p{L}][-\p{L}. ]*[\p{L}.]$");
            this.RuleFor(x => x.FamilyName).Matches(r).Unless(x => string.IsNullOrEmpty(x.FamilyName));
            this.RuleFor(x => x.GivenName).Matches(r).Unless(x => string.IsNullOrEmpty(x.GivenName));
            
            this.Include(new PersonBirthdayValidator());
            this.Include(new PersonSsnValidator());

            this.RuleFor(x => x.Insurance).NotEmpty();
            this.RuleFor(x => x.Insurance).SetValidator(new CodeValidator<Person, string, InsuranceCodeProvider>());
            
            this.RuleFor(x => x.Nationality).NotEmpty();
            this.RuleFor(x => x.Nationality).SetValidator(new CodeValidator<Person, string, CountryCodeProvider>());

            this.RuleFor(x => x.CareAllowance).NotEmpty();

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


            this.RuleFor(x => x.Gender).NotEmpty();


        }
    }
}
