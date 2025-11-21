using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Agp.Model;
using Vodamep.ReportBase;
using Vodamep.ValidationBase;

namespace Vodamep.Agp.Validation
{
    internal class ActivityTypeValidator : AbstractValidator<AgpReport>
    {
        #region Documentation
        // AreaDef: AGP
        // OrderDef: 04
        // SectionDef: Klienten-Leistung
        // StrengthDef: Fehler

        // CheckDef: PDS/AGP Leistungsarten
        // Fields: Leistungstyp, Remark: PDS-Leistungsarten erst ab 2026
        // Fields: Leistungstyp, Remark: PDS-Leistungsarten nur für Rankweil und Vorderland erlaubt
        // Fields: Leistungstyp, Remark: Keine Vermischung zwischen AGP und PDS bei einer Leistung
        // Fields: Leistungstyp, Remark: AGP: Mehrere Leistungsarten erlaubt, PDS: Nur eine Leistungsart erlaubt
        // Fields: Leistungstyp, Remark: AGP, Klientenbeobachtung/Assessment ab 2026 nicht mehr erlaubt
        // Fields: Leistungstyp, Remark: AGP, Klientenbeobachtung oder Assessment erst ab 2026 erlaubt
        // Fields: Leistungstyp, Remark: Keine gleichen Leistungstypen innerhalb einer Leistung
        // Fields: Leistungstyp, Remark: Nur ein Wechsel zwischen AGP und PDS im Monat erlaubt
        #endregion

        public ActivityTypeValidator()
        {
            var displayNameResolver = new AgpDisplayNameResolver();

            this.RuleFor(x => x).Custom((report, ctx) =>
            {
                if (report == null || report.Activities == null)
                {
                    return;
                }

                DateTime pdsStart = new DateTime(2026, 1, 1);

                // Alle activity.Entries in einer Liste sammeln, die mit Pds... beginnen
                var allPdsEntries = report.Activities
                    .Where(a => a != null && a.Entries != null && a.Entries.Any())
                    .Where(a => a.Entries.Any(IsPdsActivityType))
                    .ToList();

                // Prüfung: PDS-Leistungsarten nur für Institutionen 0004 oder 0005 erlaubt
                var allowedInstitutionIds = new[] { "0004", "0005" };
                var institutionId = report.Institution?.Id;
                var isInstitutionAllowed = !string.IsNullOrEmpty(institutionId) && allowedInstitutionIds.Contains(institutionId);

                if (!isInstitutionAllowed && allPdsEntries.Any())
                {
                    foreach (var activity in allPdsEntries)
                    {
                        var clientName = report.GetClient(activity.PersonId);
                        var date = activity.DateD;
                        ctx.AddFailure(new ValidationFailure(nameof(Activity.Entries), Validationmessages.AgpActivityPdsNotAllowedForInstitution(clientName, date.ToShortDateString())));
                    }
                }

                // Loop durch alle Activities - Prüfungen für einzelne Activities
                bool hasError = false;
                foreach (var activity in report.Activities)
                {
                    if (activity == null || activity.Entries == null || !activity.Entries.Any())
                    {
                        continue;
                    }

                    var clientName = report.GetClient(activity.PersonId);
                    var date = activity.DateD;

                    // Keine Vermischung von PDS- und Nicht-PDS-Leistungsarten innerhalb einer Aktivität
                    var hasPdsEntries = activity.Entries.Any(IsPdsActivityType);
                    var hasNonPdsEntries = activity.Entries.Any(x => !IsPdsActivityType(x));

                    if (hasPdsEntries && hasNonPdsEntries)
                    {
                        ctx.AddFailure(new ValidationFailure(nameof(Activity.Entries), Validationmessages.AgpActivityEntriesMustNotMixPdsAndNonPds(clientName, date.ToShortDateString())));
                        hasError = true;
                    }

                    // Nicht mehr als eine PDS-Leistungsart pro Aktivität
                    if (hasPdsEntries && activity.Entries.Count > 1)
                    {
                        ctx.AddFailure(new ValidationFailure(nameof(Activity.Entries), Validationmessages.AgpActivityEntriesWithPdsMustHaveSingleEntry(clientName, date.ToShortDateString())));
                        hasError = true;
                    }

                    // Ab 01.01.2026 keine Klientenbeobachtung/Assessment mehr
                    if (date >= pdsStart && activity.Entries.Contains(ActivityType.ObservationsAssessmentAt))
                    {
                        ctx.AddFailure(new ValidationFailure(nameof(Activity.Entries), Validationmessages.AgpActivityTypeObservationsAssessmentAtNotAllowedFrom2026(clientName, date.ToShortDateString())));
                        hasError = true;
                    }

                    // Vor 01.01.2026 keine Klientenbeobachtung oder Assessment
                    if (date < pdsStart && activity.Entries.Any(x =>
                            x == ActivityType.ObservationsAt || x == ActivityType.AssessmentAt))
                    {
                        ctx.AddFailure(new ValidationFailure(nameof(Activity.Entries), Validationmessages.AgpActivityTypeObservationsAndAssessmentNotAllowedBefore2026(clientName, date.ToShortDateString())));
                        hasError = true;
                    }

                    // Innerhalb einer Aktivität sind keine doppelten Leistungstypen erlaubt
                    var doubledQuery = activity.Entries.GroupBy(y => y)
                                            .Where(activityTypes => activityTypes.Count() > 1)
                                            .Select(group => group.Key);

                    if (doubledQuery.Any())
                    {
                        ctx.AddFailure(new ValidationFailure(nameof(Activity.Minutes), Validationmessages.WithinAnActivityThereAreNoDoubledActivityTypesAllowed(clientName, date.ToShortDateString())));
                        hasError = true;
                    }
                }

                // Prüfung über alle Activities im Report: Wechsel zwischen PDS und Nicht-PDS pro Klient
                // Nur ausführen, wenn oben keine Fehler aufgetreten sind
                if (hasError)
                {
                    return;
                }
                // Wechsel zwischen PDS und Nicht-PDS pro Person zählen
                // 1 Wechsel = Warnung
                // 2 Wechsel = Fehler
                var allPersonIds = report.Activities.Select(a => a.PersonId).Distinct().ToList();

                foreach (var personId in allPersonIds)
                {
                    var personClientName = report.GetClient(personId);
                    var personActivitiesList = report.Activities
                        .Where(a => a.PersonId == personId)
                        .OrderBy(a => a.DateD)
                        .ThenBy(a => a.Id)
                        .ToList();

                    if (personActivitiesList.Count < 2)
                    {
                        continue; // Kein Wechsel möglich bei weniger als 2 Activities
                    }

                    // Loop durch alle Activities dieser Person und Wechsel ermitteln
                    int switchCount = 0;
                    bool? previousHasPds = null;

                    foreach (var personActivity in personActivitiesList)
                    {
                        if (personActivity.Entries == null || !personActivity.Entries.Any())
                        {
                            continue; // Activities ohne Entries überspringen
                        }

                        var hasPds = personActivity.Entries.Any(IsPdsActivityType);

                        if (previousHasPds.HasValue && previousHasPds.Value != hasPds)
                        {
                            switchCount++;
                        }

                        previousHasPds = hasPds;
                    }

                    // Fehler nur melden, wenn Wechsel vorhanden
                    if (switchCount == 1)
                    {
                        var failure = new ValidationFailure(nameof(Activity.Entries), Validationmessages.AgpActivityPdsSwitchWarning(personClientName));
                        failure.Severity = Severity.Warning;
                        ctx.AddFailure(failure);
                    }
                    else if (switchCount >= 2)
                    {
                        var failure = new ValidationFailure(nameof(Activity.Entries), Validationmessages.AgpActivityPdsSwitchError(personClientName));
                        failure.Severity = Severity.Error;
                        ctx.AddFailure(failure);
                    }
                }
            });
        }

        private static bool IsPdsActivityType(ActivityType activityType)
        {
            return activityType.ToString().StartsWith("Pds", StringComparison.Ordinal);
        }
    }
}