using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Agp.Model;
using Vodamep.ReportBase;
using Vodamep.ValidationBase;

namespace Vodamep.Agp.Validation
{
    internal class StaffActivityValidator : AbstractValidator<StaffActivity>
    {
        public StaffActivityValidator(AgpReport report)
        {
            #region Documentation
            // AreaDef: AGP
            // OrderDef: 02
            // SectionDef: Leistung
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Leistungstyp
            // Fields: Datum
            // Fields: Leistungszeit

            // CheckDef: Erlaubte Werte
            // Fields: Leistungstyp, Remark: Leistungstypen-Liste, Url: src/Vodamep/Datasets/Agp/StaffActivityType.csv
            // Fields: Datum, Remark: Innerhalb des Meldungs-Zeitraums
            // Fields: Leistungszeit, Remark: > 0, in 5-Minuten-Schritten
            #endregion

            var displayNameResolver = new AgpDisplayNameResolver();

            this.RuleFor(x => x.Id).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueAtDateMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(x.Id)), displayNameResolver.GetDisplayName(nameof(StaffActivity)), x.DateD.ToShortDateString()));
            this.RuleFor(x => x.ActivityType).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueAtDateMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(x.ActivityType)), displayNameResolver.GetDisplayName(nameof(StaffActivity)), x.DateD.ToShortDateString()));
            this.RuleFor(x => x.Date).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueWithIdMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(x.Date)), displayNameResolver.GetDisplayName(nameof(StaffActivity)), x.Id));

            this.RuleFor(x => x.DateD)
                .SetValidator(x => new DateTimeValidator(displayNameResolver.GetDisplayName(nameof(x.Date)),
                    null, "Mitarbeiter", report.FromD, report.ToD, x.Date));

            this.RuleFor(x => x).SetValidator(x => new StaffActivityMinutesValidator(displayNameResolver.GetDisplayName(nameof(Activity.Minutes)), x.Id));

        }
    }
}