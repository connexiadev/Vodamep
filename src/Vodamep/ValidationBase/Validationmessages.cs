using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Hkpv.Model;
using Vodamep.Hkpv.Validation;

namespace Vodamep.ValidationBase
{
    internal class Validationmessages
    {
        public static string ActivityMoreThenFive => $"Es wurden mehr als 5 gemeldet.";
        public static string ActivityMoreThen350(Person p, int x) => $"Für '{p?.FamilyName} {p?.GivenName}' wurden mehr als 350 LP in einem Monat erfasst. ({x})";
        public static string TraineeMustNotContain06To10(Staff staff) => $"'{staff?.FamilyName} {staff?.GivenName} ({staff?.Id})' darf als Auszubildende/r keine medizinischen Leistungen (6-10) dokumentieren.";
        public static string IdIsNotUnique => "Der Id ist nicht eindeutig.";
        public static string PersonWithMultipleIds(StatLp.Model.Person p, IEnumerable<string> ids) => $"'{p?.FamilyName} {p?.GivenName}' wurde mehrmals mit folgenden Ids gemeldet: '{string.Join(",", ids)}'.";
        public static string IdIsMissing(string id) => $"Der Id '{id}' fehlt.";
        public static string WithoutEntry(string e, string clientName, string staff, string date) => $"Kein Eintrag '{e}': bei '{clientName}', von '{staff}', am '{date}'.";
        public static string WithoutActivity => $"Keine Aktivitäten.";
        public static string ReportBaseWithoutActivity(string clientOrStaff, string name) => $"Keine Aktivitäten für {clientOrStaff} '{name}' dokumentiert.";
        public static string ReportBaseActivityWithoutPerson(string activityId, string personId, DateTime date) => $"Eine Aktivität vom {date.ToShortDateString()} mit der ID '{activityId}' ist keiner vorhandenen Person (ID '{personId}') zugeordnet.";
        public static string ReportBaseActivitWithoutStaff(string activityId, string staffId, DateTime date) => $"Eine Aktivität vom {date.ToShortDateString()} mit der ID '{activityId}' ist keinem vorhandenen Mitarbeiter (ID '{staffId}') zugeordnet.";
        public static string StaffWithoutEmployment => $"Beim Mitarbeiter ist keine Beschäftigung vorhanden";
        public static string EmploymentHoursPerWeekMustBeBetween0And100 => $"Die Stundenanzahl muss größer 0 und kleiner 100 sein";
        public static string InvalidEmploymentFromToReportRange(Staff data) => $"Das Anstellungsverhältniss von {data.GivenName} {data.FamilyName} muss innerhalb des Meldungszeitraums liegen.";
        public static string EmploymentOverlap(Staff data) => $"Die Anstellungsverhälnisse bei {data.GivenName} {data.FamilyName} überschneiden sich.";
        public static string InvalidEmploymentForActivity(Staff staff, int count, DateTime minDate, DateTime maxDate) => $"Für Mitarbeiter {staff.GivenName} {staff.FamilyName} gibt es kein Anstellungsverhältnis für {GetActivityPlural(count)} {GetRange(minDate, maxDate)}.";
        public static string BirthdayNotInFuture => "'Geburtsdatum' darf nicht in der Zukunft liegen.";
        public static string BirthdayNotInSsn(Person data) => $"Das Geburtsdatum {data?.BirthdayD.ToString("dd.MM.yyyy")} unterscheidet sich vom Wert in der Versicherungsnummer {SSNHelper.Format(data?.Ssn, true).Substring(5)}.";
        public static string SsnNotValid => "Die Versicherungsnummer {PropertyValue} ist nicht korrekt.";
        public static string SsnNotUnique(IEnumerable<Person> p) => $"Mehrere Personen haben die selbe Versicherungsnummer {p?.FirstOrDefault()?.Ssn}: {string.Join(", ", p?.Select(x => $"{x.GivenName} {x.FamilyName}({x.Id})"))}";
        public static string DateMustnotHaveTime => "'{PropertyName}' darf keine Uhrzeit beinhalten.";
        public static string OneMonth => "Die Meldung muss genau einen Monat beinhalten.";
        public static string SameYear => "Die Meldung darf keinen Jahreswechsel beinhalten.";
        public static string LastDateInMonth => "'{PropertyName}' muss der letzte Tag des Monats sein.";
        public static string LastDateInYear => "'{PropertyName}' muss der letzte Tag des Jahres sein.";
        public static string FirstDateInMonth => "'{PropertyName}' muss der erste Tag des Monats sein.";
        public static string FirstDateInYear => "'{PropertyName}' muss der erste Tag des Jahres sein.";
        public static string InvalidCode => "Für '{PropertyName}' ist '{PropertyValue}' kein gültiger Code.";
        public static string ReportBaseInvalidCode(string clientOrStaff, string client) => $"Für '{{PropertyName}}' von {clientOrStaff} '{client}' ist '{{PropertyValue}}' kein gültiger Code.";
        public static string InvalidPostCode_City => "'{PropertyValue}' ist kein gültiger Ort.";
        public static string ReportBaseInvalidPostCodeCity(string client) => $"'{{PropertyValue}}' ist kein gültiger Ort von Klient '{client}'.";
        public static string ReportBaseReferrerIsOtherRefererrerThenOtherReferrerMustBeSet(string client, string clientOrStaff) => $"Wenn der Zuweiser von {clientOrStaff} '{client}' ein Anderer Zuweiser ist, dann muss Anderer Zuweiser gesetzt sein.";
        public static string DoubledDiagnosisGroups(string client) => $"Es dürfen keine doppelten Diagnosegruppen für Klient '{client}' vorhanden sein.";
        public static string OnlyONePalliativeDiagnosisGroup(string client) => $"Es darf nur eine Palliativ Diagnose Gruppe für Klient '{client}' vorhanden sein.";
        public static string AtLeastOneDiagnosisGroup(string client) => $"Es muss mindestens eine Diagnosegruppe für Klient '{client}' vorhanden sein.";
        public static string ReportBaseMinutesMustBeGreater0(string propertyName, string clientId) => $"{propertyName} von Klient '{clientId}' muss größer 0 sein.";
        public static string StaffActivityMinutesMustBeGreater0(string propertyName, string id) => $"{propertyName} mit Id '{id}' muss größer 0 sein. Wenden Sie sich an Ihren Support.";
        public static string ReportBaseStepWidthWrong(string propertyName, string clientName, int nrOfMinutes) => $"{propertyName} von '{clientName}' darf nur in {nrOfMinutes} Minuten Schritten eingegeben werden.";
        public static string ReportBaseMaxSumOfMinutesPerStaffMemberIs12Hours(string date, string name) => $"Die Leistungsminuten von '{name}' am '{date}' dürfen 12 Stunden nicht überschreiten.";
        public static string MaxSumOfMinutesTravelTimesIs5Hours(string staff, string date) => $"Summe Wegzeiten von Mitarbeiter {staff} am {date} darf 12 Stunden nicht überschreiten.";
        public static string OnlyOneTravelTimeEntryPerStaffMemberAndDay => "Pro Mitarbeiter ist nur ein Eintrag bei den Wegzeiten pro Tag erlaubt.";
        public static string WithinAnActivityThereAreNoDoubledActivityTypesAllowed(string client, string date) => $"Innerhalb einer Aktivität (Klient '{client}' am {date}) dürfen keine doppelten Leistungstypen vorhanden sein.";
        public static string WithinAnActivityTheValuesAreNotAllowedInCombination(string client, string activityType1, string activityType2) => $"Innerhalb einer Aktivität von Klient '{client}' dürfen nicht gleichzeitg die Leistungstypen '{activityType1}' und '{activityType2}' vorhanden sein.";
        public static string InvalidInstitutionNumber => "Ungültige Einrichtungsnummer.";
        public static string NoDoubledValuesAreAllowed => $"Doppelte Angaben bei '{{PropertyName}}'";
        public static string ItemNotValid => "Keine gültige Angabe bei '{PropertyName}'";
        public static string TextAreaEnterAValue => "Bei '{PropertyName}' im Textfeld bitte einen Wert angegeben.";
        public static string PersonIsNotAvailable(string name) => $"Person '{name}' ist nicht in der Personenliste vorhanden.";
        public static string FromMustBeBeforeTo => $"'Von' muss vor 'Bis' liegen.";
        public static string DeadClientNeedsDeadthLocation() => $"Wenn der Klient gestorben ist, muss eine Angabe zum Sterbeort gemacht werden.";
        public static string DeadClientMustNotContainDischargeLocation() => $"Wenn der Klient gestorben ist, darf keine Angabe zur Entlassung gemacht werden.";
        public static string LeavingOtherFilledNeedsOther() => $"Wenn bei der Entlassung sonstige Angaben gemacht werden, muss 'Sonstige' ausgewählt werden.";
        public static string LeavingClientNeedsLeavingLocation() => $"Wenn der Klient entlassen worden ist, muss angegeben werden, wohin der Klient entlassen wurde.";
        public static string LeavingClientNeedsLeavingReason() => $"Wenn der Klient entlassen worden ist, muss angegeben werden, warum der Klient entlassen wurde.";
        public static string LeavingClientDeathMustNotBeFilled() => $"Wenn der Klient entlassen worden ist, darf keine Angabe zum Sterbefall gemacht werden.";
        public static string DischargedClientNeedsDischargeLocation() => $"Wenn der Klient entlassen worden ist, muss angegeben werden, wohin der Klient entlassen wurde.";
        public static string InvalidValue() => $"Ungültiger Wert für '{{PropertyName}}'.";
        public static string TextTooLong() => $"Zu langer Text für '{{PropertyName}}'.";
        public static string ReportBaseIdIsNotUnique(string clientId) => $"Die Id von Klient '{clientId}' ist nicht eindeutig.";
        public static string ReportBaseActivityDateMustBeWithinReport(string propertyName, string clientPersonName, string dateTime) => $"'{propertyName}' der Aktivität {dateTime} von '{clientPersonName}' muss innerhalb des Meldungszeitraums liegen.";
        public static string ReportBaseBirthdayNotInFuture(string clientId, string clientOrStaff) => $"'Geburtsdatum' von {clientOrStaff} '{clientId}' darf nicht in der Zukunft liegen.";
        public static string ReportBaseBirthdayMustNotBeBefore(string clientId, string clientOrStaff) => $"Der Wert von 'Geburtsdatum' von {clientOrStaff} '{clientId}' muss grösser oder gleich .*.";
        public static string ReportBaseValueMustNotBeEmpty(string clientId) => $"'{{PropertyName}}' von Klient '{clientId}' darf nicht leer sein.";

        public static string ReportBaseClientValueMustNotBeEmpty(string property, string clientId) => $"'{property}' von Klient '{clientId}' darf nicht leer sein.";
        public static string ReportBaseClientInvalidValue(string property, string id) => $"'{property}' von Klient '{id}' hat einen ungültigen Wert'";


        public static string ReportBaseValueAtDateMustNotBeEmpty(string property, string parentProperty, string date) => $"{property} bei {parentProperty} vom {date} darf nicht leer sein.";
        public static string ReportBaseValueWithIdMustNotBeEmpty(string property, string parentProperty, string id) => $"{property} bei {parentProperty} mit ID {id} darf nicht leer sein.";


        public static string ClientWrongPostCodeCity(string clientName) => $"Ungültige Kombination Ort/Plz bei Klient {clientName}.";



        public static string ReportBaseValueMustNotBeEmptyWithProperty(string property) => $"'{{PropertyName}}' von '{property}' darf nicht leer sein.";
        public static string ReportBaseValueMustNotBeEmpty(string clientOrStaff, string clientId) => $"'{{PropertyName}}' von {clientOrStaff} '{clientId}' darf nicht leer sein.";
        public static string ReportBaseValueMustNotBeEmptyWithParentProperty(string property, string parentProperty) => $"'{property}' von {parentProperty} darf nicht leer sein.";
        public static string ReportBaseValueMustNotBeEmptyWithString(string clientId) => $"Ein Merkmal von Klient '{clientId}' darf nicht leer sein.";
        public static string ReportBaseValueMustNotBeEmpty(string property, string parentProperty, string clientId) => $"'{property}' von {parentProperty} von Klient '{clientId}' darf nicht leer sein.";
        public static string ReportBaseValueMustBeGreaterThanZero(string property, string parentProperty, string clientOrStaff, string clientId) => $"Der Wert von'{property}' von {parentProperty} von {clientOrStaff} '{clientId}' muss größer 0 sein.";
        public static string ReportBasePropertyInvalidFormat(string clientOrStaff, string clientOrStaffId) => $"'{{PropertyName}}' von {clientOrStaff} '{clientOrStaffId}' weist ein ungültiges Format auf.";
        public static string ReportBaseInvalidLength(string id) => $"'{{PropertyName}}' von Klient '{id}' besitzt eine ungültige Länge'";
        public static string ReportBaseInvalidValue(string id) => $"'{{PropertyName}}' von Klient '{id}' hat einen ungültigen Wert'";
        public static string ReportBaseDateMustNotHaveTime(string id, string clientOrStaff) => $"'{{PropertyName}}' von {clientOrStaff} '{id}' darf keine Uhrzeit beinhalten.";
        public static string ReportBaseActivityWrongValue(string date, string op) => $"Bei der Leistung am {date} wurde ein Wert {op} angegeben.";
        public static string ReportBasePersonActivityWrongValue(string personId, string op) => $"Bei der Leistung von Person '{personId}' wurde ein Wert {op} angegeben.";
        public static string ReportBasePersonActivityWrongValue(string personId, string date, string op) => $"Bei der Leistung von Person '{personId}' am {date} wurde ein Wert {op} angegeben.";
        public static string ReportBaseActivityWrongStepLength(string personId, string stepLength) => $"Bei der Leistung von Person '{personId}' muss der Wert in {stepLength} Schritten angegeben werden.";
        public static string ReportBaseActivityNoCategory(string date) => $"Bei der Leistung am {date} wurde keine Kategorie angegeben.";
        public static string ReportBasePersonActivityNoCategory(string personId, string date) => $"Bei der Leistung von Person '{personId}' am {date} wurde keine Kategorie angegeben.";
        public static string ReportBaseActivityWrongDate(string date) => $"Bei der Leistung am {date} wurde ein Datum außerhalb des Meldungszeitraums angegeben.";
        public static string ReportBasePersonActivityWrongDate(string personId, string date) => $"Bei der Leistung von Person '{personId}' am {date} wurde ein Datum außerhalb des Meldungszeitraums angegeben.";
        public static string ReportBaseClientActivityUnknownPerson(string date) => $"Unbekannter Klient bei Leistung am {date}.";
        public static string ReportBaseActivityMultipleActivitiesForOnePerson(string personId) => $"Mehrfache Leistungen für Klient '{personId}' vorhanden.";
        public static string ReportBaseActivityContainsNonExistingPerson(string personId) => $"Für Klient '{personId}' wurden keine Personendaten gesendet.";
        public static string ReportBaseItemMustBeInReportPeriod(string clientName) => $"{{PropertyName}} von Person '{clientName}' muss im Meldezeitraum liegen.";

        public static string StatLpAdmissionInvalidValueAdmission(string date) => $"Ungültiger Wert für '{{PropertyName}}' bei Aufnahme am {date}.";
        public static string StatLpAdmissionTextTooLong(string date) => $"Zu langer Text für '{{PropertyName}}' bei Aufnahme am {date}.";
        public static string StatLpAdmissionEmptyPostCode(string date) => $"Keine Angabe von Ort/Plz bei Aufnahme am {date}.";
        public static string StatLpAdmissionWrongPostCode(string date) => $"Ungültige Kombination Ort/Plz bei Aufnahme am {date}.";

        public static string StatLpAdmissionAttributeEmpty(string clientName, string date, string attributeType) => $"Für die Person '{clientName}' wurde am {date} kein Wert bei '{attributeType}' gemeldet.";

        public static string StatLpAdmissionAttributeMissing(string clientName, string date, string attributeType) => $"Für die Person '{clientName}' wurde am {date} keine '{attributeType}' gemeldet.";
        public static string StatLpAttributeMultiple(string clientName, string date, string attributeType) => $"Beim Klient '{clientName}' wurde am {date} das Attribut '{attributeType}' mehrfach angegeben.";

        public static string StatLpAttributeNotWithinDate(string clientName, string date, string attributeType) => $"Beim Klient '{clientName}' wurde am {date} das Attribut '{attributeType}' außerhalb eines Aufenthaltes angegeben.";

        public static string StatLpStayEveryPersonMustBeInAStay(string clientName) => $"Die Person '{clientName}' wird in keinem Aufenthalt erwähnt.";

        public static string StatLpStayAheadOfPeriod(string clientName, string fromDate, string toDate) => $"Der Aufenthalt vom '{fromDate}' bis zum '{toDate}' von '{clientName}' liegt vor dem Meldunszeitraum.";

        public static string StatLpStayAfterPeriod(string clientName, string fromDate, string toDate) => $"Der Aufenthalt vom '{fromDate}' bis zum '{toDate}' von '{clientName}' liegt nach dem Meldunszeitraum.";

        public static string StatLpMissingAdmission(string clientName, string fromDate) => $"Für die Neuaufnahme vom '{fromDate}' von '{clientName}' wurden keine Aufnahmedaten gemeldet.";

        public static string StatLpMissingLeaving(string clientName, string fromDate) => $"Für die Entlassung am '{fromDate}' von '{clientName}' wurden keine Entlassungsdaten gemeldet.";

        public static string StatLpMultipleAdmission(string clientName, string fromDate) => $"Für die Neuaufnahme vom '{fromDate}' von '{clientName}' wurden mehrere Aufnahmedaten gemeldet.";

        public static string StatLpMultipleLeavings(string clientName, string fromDate) => $"Für die aufnahme vom '{fromDate}' von '{clientName}' wurden mehrere Entlassungsdaten gemeldet.";

        public static string StatLpStayToLong(string type, string clientName, string fromDate, string toDate, int maxDays) => $"Der Aufenthalt '{type}' vom '{fromDate}' bis zum '{toDate}' von '{clientName}' darf nicht länger als {maxDays} Tage dauern.";

        public static string StatLpInvalidAdmissionTypeChange(string fromType, string toType, string clientName, string fromDate, string toDate) => $"Nach dem Aufenthalt '{fromType}' vom '{fromDate}' bis zum '{toDate}' von '{clientName}' darf kein '{toType}' folgen.";

        public static string StatLpLeavingReasonMustnotBeEmpty() => $"Beim Abgang muss eine Abgang Art angegeben werden.";

        public static string StatLpAttributeInvalidAdmissionType(string clientName, string admissionType, string date) => $"Die Aufnahmeart {admissionType} bei der Aufnahme vom {date} von Klient '{clientName}' ist nicht mehr erlaubt.";

        public static string PersonsPropertyDiffers(string name, string propertyName, string curValue, string preValue) => $"Für die Person '{name}' wurde für '{propertyName}' bisher '{preValue}' gemeldet. Jetzt ist der Wert '{curValue}'.";

        public static string MkkpActivityScopeEmpty(string clientName, string date) => $"Leistungsbereich für Aktivität vom {date} '{clientName}' nicht angegeben.";


        public static string AgpActivityEntriesMustNotMixPdsAndNonPds(string client, string date) => $"Leistungsbereiche AGP und PDS dürfen am gleichen Tag nicht gemeinsam angegeben werden (Klient '{client}' am {date}).";

        public static string AgpActivityEntriesWithPdsMustHaveSingleEntry(string client, string date) => $"Eine PDS-Leistung darf nur eine Tätigkeit beinhalten (Klient '{client}' am {date}).";

        public static string AgpActivityTypeObservationsAssessmentAtNotAllowedFrom2026(string client, string date) => $"Klientenbeobachtung/Assessment ab 01.01.2026 nicht mehr erlaubt (Klient '{client}' am {date}).";

        public static string AgpActivityTypeObservationsAndAssessmentNotAllowedBefore2026(string client, string date) => $"Klientenbeobachtung oder Assessment (einzeln) erst ab 01.01.2026 erlaubt (Klient '{client}' am {date}).";
        
        public static string AgpActivityPdsSwitchWarning(string client) => $"Klient '{client}' hat einen Wechsel zwischen den Leistungsbereichen AGP/PDS.";
        public static string AgpActivityPdsSwitchError(string client) => $"Mehrere Wechsel für Klient '{client}' zwischen den Leistungsbereichen AGP und PDS sind nicht erlaubt.";

        public static string GetRange(DateTime minDate, DateTime maxDate)
        {
            if (minDate == maxDate)
            {
                return $"am {minDate.ToShortDateString()}";
            }
            else
            {
                return $"zwischen {minDate.ToShortDateString()} und {maxDate.ToShortDateString()}";
            }
        }


        public static string GetActivityPlural(int count)
        {
            if (count > 1)
            {
                return $"{count} Leistungen";
            }
            else
            {
                return $"{count} Leistung";
            }
        }

    }
}
