using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Vodamep.Agp.Model;
using Vodamep.Data;
using Vodamep.ValidationBase;

namespace Vodamep.Agp.Validation
{
    internal class AgpPersonValidator : AbstractValidator<Person>
    {
        public AgpPersonValidator()
        {
            AgpDisplayNameResolver displayNameResolver = new AgpDisplayNameResolver();

            #region Documentation
            // AreaDef: AGP
            // OrderDef: 01
            // SectionDef: Person
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Geschlecht
            // Fields: Pflegestufe
            // Fields: Zuweiser
            // Fields: Sonstiger Zuweiser, Remark: Wenn Zuweiser = Sonstiger
            // Fields: PLZ/Ort
            // Fields: Staatsbürgerschaft
            // Fields: Diagnosegruppen, Remark: Mindestens 1 Eintrag

            // CheckDef: Erlaubte Werte
            // Fields: Geschlecht, Remark: Geschlechter-Liste, Url: src/Vodamep/Datasets/Gender.csv
            // Fields: Pflegestufen, Remark: Pflegestufen-Liste, Url:  src/Vodamep/Datasets/CareAllowance.csv
            // Fields: Zuweiser, Remark: Zuweiser-Liste, Url: src/Vodamep/Datasets/Agp/Referrer.csv
            // Fields: PLZ/Ort, Remark: PLZ/Orte-Liste, Url: src/Vodamep/Datasets/PostcodeCity.csv
            // Fields: Staatsbürgerschaft, Remark: Staatsbürgerschaften-Liste, Url: src/Vodamep/Datasets/CountryCode.csv
            // Fields: Diagnosegruppen, Remark: Diagnosegruppen-Liste, Url: src/Vodamep/Datasets/Agp/Diagnosisgroup.csv
            // Fields: Versicherung, Remark: Versicherungs-Liste, Url: src/Vodamep/Datasets/InsuranceCode.csv
            #endregion

            this.RuleFor(x => x.Gender).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));

            this.RuleFor(x => x.CareAllowance).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.Referrer).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.OtherReferrer).NotEmpty()
                                    .When(y => y.Referrer == Referrer.OtherReferrer)
                                    .WithMessage(x => Validationmessages.ReportBaseReferrerIsOtherRefererrerThenOtherReferrerMustBeSet(x.GetDisplayName(), displayNameResolver.GetDisplayName(nameof(Person))));

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

            this.RuleFor(x => x.Nationality).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(x.GetDisplayName()));
            this.RuleFor(x => x.Nationality)
                .Must((person, country) => CountryCodeProvider.Instance.IsValid(country))
                .Unless(x => string.IsNullOrWhiteSpace(x.Nationality))
                .WithMessage(x => Validationmessages.ReportBaseInvalidValue(x.GetDisplayName()));

            this.RuleFor(x => x.Diagnoses).NotEmpty().WithMessage(x => Validationmessages.AtLeastOneDiagnosisGroup(x.GetDisplayName()));

            this.RuleFor(x => x.Insurance).SetValidator(new CodeValidator<InsuranceCodeProvider>()).Unless(x => string.IsNullOrEmpty(x.Insurance)).WithMessage(x => Validationmessages.ReportBaseInvalidCode(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.Insurance).SetValidator(new CodeValidator<Person, string, InsuranceCodeProvider>()).Unless(x => string.IsNullOrEmpty(x.Insurance)).WithMessage(x => Validationmessages.ReportBaseInvalidCode(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.Diagnoses).NotEmpty().WithMessage(x => Validationmessages.AtLeastOneDiagnosisGroup(x.GetDisplayName()));

            this.Include(new DiagnosisGroupIsUniqueValidator());
        }
    }
}