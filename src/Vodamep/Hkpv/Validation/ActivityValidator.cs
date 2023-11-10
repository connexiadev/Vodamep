using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class ActivityValidator : AbstractValidator<Activity>
    {
        public ActivityValidator(DateTime from, DateTime to)
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 04
            // SectionDef: Leistung
            // StrengthDef: Fehler

            // CheckDef: Muss Feld
            // Fields: Datum
            // Fields: Mitarbeiter
            // Fields: Klient
            // Fields: Leistungen, Remark: Mindestens 1 Eintrag

            // CheckDef: Erlaubte Werte
            // Fields: Datum, Remark: Innerhalb des Meldungs-Zeitraums
            // Fields: Leistungstyp, Remark: Leistungstypen-Liste, Url: src/Vodamep/Datasets/Hkpv/ActivityType.csv
            #endregion

            this.RuleFor(x => x.Date).NotEmpty();
            this.RuleFor(x => x.Date).SetValidator(new TimestampWithOutTimeValidator()).Unless(x => x.Date == null);

            if (from != DateTime.MinValue)
            {
                this.RuleFor(x => x.DateD).GreaterThanOrEqualTo(from).Unless(x => x.Date == null);
            }
            if (to > from)
            {
                this.RuleFor(x => x.DateD).LessThanOrEqualTo(to).Unless(x => x.Date == null);
            }

            this.RuleFor(x => x.StaffId).NotEmpty();

            this.RuleFor(x => x.PersonId).NotEmpty().Unless(x => x.WithoutPersonId());
            this.RuleFor(x => x.PersonId).Empty().Unless(x => x.RequiresPersonId());


            this.RuleFor(x => x.Entries).NotEmpty();
            this.RuleForEach(x => x.Entries).NotEqual(ActivityType.UndefinedActivity);
        }
    }

}
