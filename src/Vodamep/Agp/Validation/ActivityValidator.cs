using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Agp.Model;
using Vodamep.ReportBase;
using Vodamep.ValidationBase;

namespace Vodamep.Agp.Validation
{
    internal class ActivityValidator : AbstractValidator<Activity>
    {
        public ActivityValidator(AgpReport report)
        {
            #region Documentation
            // AreaDef: AGP
            // OrderDef: 03
            // SectionDef: Klienten-Leistung
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Klienten-Aktivitäts-Typ
            // Detail: Einsatzort
            // Detail: Datum
            // Detail: Leistungszeit

            // CheckDef: Erlaubte Werte
            // Detail: Aktivitäts-Typ, Ref: Aktivitäts-Typen-Liste, Url: src/Vodamep/Datasets/Agp/ActivityType.csv
            // Detail: Einsatzort, Ref: Einsatzort-Liste, Url: src/Vodamep/Datasets/Agp/PlaceOfAction.csv
            // Detail: Datum, Remark: Innerhalb des Berichts-Zeitraums
            // Detail: Leistungszeit, Remark: Größer 0, in 5 Minuten-Schritten
            #endregion

            var displayNameResolver = new AgpDisplayNameResolver();

            this.RuleFor(x => x.PersonId).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmptyWithParentProperty(displayNameResolver.GetDisplayName(nameof(x.PersonId)), displayNameResolver.GetDisplayName(nameof(Activity))));

            this.RuleFor(x => x.Id).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(x.Id)), displayNameResolver.GetDisplayName(nameof(Activity)), report.GetClient(x.PersonId)));
            this.RuleFor(x => x.PlaceOfAction).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(x.PlaceOfAction)), displayNameResolver.GetDisplayName(nameof(Activity)), report.GetClient(x.PersonId)));
            this.RuleFor(x => x.Entries).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(x.Entries)), displayNameResolver.GetDisplayName(nameof(Activity)), report.GetClient(x.PersonId)));
            this.RuleFor(x => x.Date).NotEmpty()
                .WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmpty(displayNameResolver.GetDisplayName(nameof(x.Date)), displayNameResolver.GetDisplayName(nameof(Activity)), report.GetClient(x.PersonId)));


            this.RuleFor(x => x.DateD)
                .SetValidator(x => new DateTimeValidator(displayNameResolver.GetDisplayName(nameof(x.Date)),
                    report.GetClient(x.PersonId), null, report.FromD, report.ToD, x.Date));


            this.RuleFor(x => x).Custom((x, ctx) =>
            {
                var entries = x.Entries;

                var doubledQuery = entries.GroupBy(y => y)
                    .Where(activityTypes => activityTypes.Count() > 1)
                    .Select(group => group.Key);

                if (doubledQuery.Any())
                {
                    ctx.AddFailure(new ValidationFailure(nameof(Activity.Minutes), Validationmessages.WithinAnActivityThereAreNoDoubledActivityTypesAllowed(report.GetClient(x.PersonId))));
                }

            });

            this.RuleFor(x => x).SetValidator(x => new ActivityMinutesValidator(displayNameResolver.GetDisplayName(nameof(Activity.Minutes)), report.GetClient(x.PersonId)));
        }
    }
}