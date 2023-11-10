using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Vodamep.Data;
using Vodamep.Mkkp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Mkkp.Validation
{
    internal class MkkpPersonValidator : AbstractValidator<Person>
    {
        public MkkpPersonValidator()
        {

            #region Documentation
            // AreaDef: MKKP
            // OrderDef: 01
            // SectionDef: Klient
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Geburtsdatum
            // Detail: Zuweiser
            // Detail: Sonstiger Zuweiser, Remark: Wenn Zuweiser = Sonstiger
            // Detail: Arzt/Krankenhaus
            // Detail: Arzt/Niedergelassen
            // Detail: Versicherung
            // Detail: Pflegestufe
            // Detail: Geschlecht
            // Detail: PLZ/Ort

            // CheckDef: Erlaubte Werte
            // Detail: Arzt/Krankenhaus, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            // Detail: Arzt/Niedergelassen, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            // Detail: Versicherung, Remark: Versicherungs-Liste, Url: src/Vodamep/Datasets/InsuranceCode.csv
            // Detail: PLZ/Ort, Remark: PLZ/Orte-Liste, Url: src/Vodamep/Datasets/PostcodeCity.csv
            // Detail: Geschlecht, Remark: Geschlechter-Liste, Url: src/Vodamep/Datasets/Gender.csv
            // Detail: Pflegestufen, Remark: Pflegestufen-Liste, Url:  src/Vodamep/Datasets/CareAllowance.csv
            // Detail: Zuweiser, Remark: Zuweiser-Liste, Url: src/Vodamep/Datasets/Mkkp/Referrer.csv
            // Detail: Diagnosegruppen, Remark: Diagnosegruppen-Liste, Url: src/Vodamep/Datasets/Mkkp/Diagnosisgroup.csv
            #endregion

            MkkpDisplayNameResolver displayNameResolver = new MkkpDisplayNameResolver();

            this.RuleFor(x => x.Birthday).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.Referrer).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.HospitalDoctor).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.LocalDoctor).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.Insurance).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.CareAllowance).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.Gender).NotEmpty().WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));

            // Änderung 5.11.2018, LH
            var r = new Regex(@"^[\p{L}][-\p{L}. ]*[\p{L}.]$");
            this.RuleFor(x => x.HospitalDoctor).Matches(r).Unless(x => string.IsNullOrEmpty(x.HospitalDoctor)).WithMessage(x => Validationmessages.ReportBasePropertyInvalidFormat(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));
            this.RuleFor(x => x.LocalDoctor).Matches(r).Unless(x => string.IsNullOrEmpty(x.LocalDoctor)).WithMessage(x => Validationmessages.ReportBasePropertyInvalidFormat(displayNameResolver.GetDisplayName(nameof(Person)), x.GetDisplayName()));

            this.RuleFor(x => x.Insurance).SetValidator(new CodeValidator<InsuranceCodeProvider>());

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

            this.RuleFor(x => x.OtherReferrer).NotEmpty()
                .When(y => y.Referrer == Referrer.OtherReferrer)
                .WithMessage(x => Validationmessages.ReportBaseReferrerIsOtherRefererrerThenOtherReferrerMustBeSet(x.GetDisplayName(), displayNameResolver.GetDisplayName(nameof(Person))));
         
            this.RuleFor(x => x.Diagnoses).NotEmpty().WithMessage(x => Validationmessages.AtLeastOneDiagnosisGroup(x.GetDisplayName()));
            this.Include(new DiagnosisGroupIsUniqueValidator());
            this.Include(new DiagnosisGroupMustNotContainUndefinedValueValidator());
            this.Include(new DiagnosisGroupOnlyOnePalliativCareValidator());
        }

    }
}