using FluentValidation;
using System.Text.RegularExpressions;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class LeavingValidator : AbstractValidator<Leaving>
    {
        private static readonly DisplayNameResolver DisplayNameResolver = new DisplayNameResolver();

        public LeavingValidator(StatLpReport report)
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 05
            // SectionDef: Abgang
            // StrengthDef: Hart

            // CheckDef: Muss Feld
            // Detail: Abgangsgrund
            // Detail: Sterbefall, Remark: Wenn Abgangsgrund = verstorben
            // Detail: Abgangsdatum
            // Detail: Entlassung Wohin, Remark: Wenn Abgangsgrund = Entlassung
            // Detail: Entlassung Wohin Sontige, Remark: Wenn Entlassung Wohin = Sonstige
            // Detail: Entlassung Grund, Remark: Wenn Abgangsgrund = Entlassung
            // Detail: Entlassung Grund Anderer, Remark: Wenn Entlassung Grund = Sonstiger

            // CheckDef: Erlaubte Werte
            // Detail: Abgangsgrund, Remark: Abgangs-Gründe, Url: src/Vodamep/Datasets/StatLp/LeavingReason.csv
            // Detail: Abgangsdatum, Remark: Innerhalb des Meldungszeitraums
            // Detail: Sterbefall, Remark: Sterbefall-Liste, Url: src/Vodamep/Datasets/StatLp/DeathLocation.csv
            // Detail: Entlassung Wohin, Remark: Entlassungs-Liste, Url: src/Vodamep/Datasets/StatLp/DischargeLocation.csv
            // Detail: Entlassung Grund, Remark: Entlassungs-Liste, Url: src/Vodamep/Datasets/StatLp/DischargeReason.csv
            #endregion

            this.RuleFor(x => x.LeavingReason).NotEmpty().WithMessage(x => Validationmessages.StatLpLeavingReasonMustnotBeEmpty());

            this.RuleFor(x => x.DeathLocation).NotEmpty()
                .Unless(x => x.LeavingReason != LeavingReason.DeceasedLr)
                .Unless(x => x.LeavingReason == LeavingReason.UndefinedLr)
                .WithMessage(x => Validationmessages.DeadClientNeedsDeadthLocation());

            this.RuleFor(x => x.LeavingDate).NotEmpty();

            if (report.From != null && report.To != null)
            {
                this.RuleFor(x => x.LeavingDate)
                    .Must(x => report.From <= x && x <= report.To)
                    .Unless(x => x.LeavingDate == null)
                    .WithName(DisplayNameResolver.GetDisplayName(nameof(Leaving)))
                    .WithMessage(x => Validationmessages.ReportBaseItemMustBeInReportPeriod(report.GetPersonName(x.PersonId)));
            }

            this.RuleFor(x => x).Must(x =>
            {
                if (x.LeavingReason == LeavingReason.DeceasedLr)
                {
                    if (x.DischargeLocation != DischargeLocation.UndefinedDc)
                        return false;

                    if (!string.IsNullOrEmpty(x.DischargeLocationOther))
                        return false;

                    if (x.DischargeReason != DischargeReason.UndefinedDr)
                        return false;

                    if (!string.IsNullOrEmpty(x.DischargeReasonOther))
                        return false;
                }

                return true;

            }).WithMessage(x => Validationmessages.DeadClientMustNotContainDischargeLocation());

            this.RuleFor(x => x.DischargeReasonOther).Empty()
                .Unless(x => x.DischargeReason == DischargeReason.OtherDr)
                .WithMessage(x => Validationmessages.LeavingOtherFilledNeedsOther());

            this.RuleFor(x => x.DischargeLocationOther).Empty()
                 .Unless(x => x.DischargeLocation == DischargeLocation.OtherDc)
                 .WithMessage(x => Validationmessages.LeavingOtherFilledNeedsOther());

            this.RuleFor(x => x.DischargeLocation).NotEmpty()
               .When(x => x.LeavingReason == LeavingReason.DischargeLr)
               .WithMessage(x => Validationmessages.LeavingClientNeedsLeavingLocation());

            this.RuleFor(x => x.DischargeReason).NotEmpty()
                .When(x => x.LeavingReason == LeavingReason.DischargeLr)
                .WithMessage(x => Validationmessages.LeavingClientNeedsLeavingReason());

            this.RuleFor(x => x.DeathLocation).Empty()
                .When(x => x.LeavingReason != LeavingReason.DeceasedLr)
                .WithMessage(x => Validationmessages.LeavingClientDeathMustNotBeFilled());

            this.RuleFor(x => x.DischargeLocation).NotEmpty()
                .When(x => x.LeavingReason == LeavingReason.DischargeLr)
                .WithMessage(x => Validationmessages.DischargedClientNeedsDischargeLocation());

            var r = new Regex(@"^[-,.a-zA-Z0-9äöüÄÖÜß\(\) ][-,.a-zA-Z0-9äöüÄÖÜß\(\) ]*[-,.a-zA-Z0-9äöüÄÖÜß\(\) ]$");

            this.RuleFor(x => x.DischargeLocationOther)
                .Matches(r).Unless(x => string.IsNullOrEmpty(x.DischargeLocationOther))
                .WithName(x => DisplayNameResolver.GetDisplayName(nameof(x.DischargeLocationOther)))
                .WithMessage(x => Validationmessages.InvalidValue());

            this.RuleFor(x => x.DischargeLocationOther)
                .MaximumLength(30).Unless(x => string.IsNullOrEmpty(x.DischargeLocationOther))
                .WithName(x => DisplayNameResolver.GetDisplayName(nameof(x.DischargeLocationOther)))
                .WithMessage(x => Validationmessages.TextTooLong());

            this.RuleFor(x => x.DischargeReasonOther)
                .Matches(r).Unless(x => string.IsNullOrEmpty(x.DischargeReasonOther))
                .WithName(x => DisplayNameResolver.GetDisplayName(nameof(x.DischargeReasonOther)))
                .WithMessage(x => Validationmessages.InvalidValue());

            this.RuleFor(x => x.DischargeReasonOther)
                .MaximumLength(30).Unless(x => string.IsNullOrEmpty(x.DischargeReasonOther))
                .WithName(x => DisplayNameResolver.GetDisplayName(nameof(x.DischargeReasonOther)))
                .WithMessage(x => Validationmessages.TextTooLong());

        }
    }
}