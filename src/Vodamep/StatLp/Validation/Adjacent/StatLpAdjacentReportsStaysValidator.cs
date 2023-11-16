using FluentValidation;
using FluentValidation.Results;
using System;
using System.Linq;
using Vodamep.StatLp.Model;

namespace Vodamep.StatLp.Validation.Adjacent
{
    internal class StatLpAdjacentReportsStaysValidator : AbstractValidator<(StatLpReport Predecessor, StatLpReport Report)>
    {

        private static readonly DisplayNameResolver DisplayNameResolver = new DisplayNameResolver();

        static StatLpAdjacentReportsStaysValidator()
        {
            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
        }

        public StatLpAdjacentReportsStaysValidator()
        {
            this.RuleFor(x => x).Custom(CheckStays);
        }

        #region Documentation
        // AreaDef: STAT
        // OrderDef: 03
        // SectionDef: Aufenthalt
        // StrengthDef: Warnung
        // LocationDef: Eingang
        // Fields: Von/Bis/Aufnahmeart, Check: Aufenthalts-Zusammenhang, Remark: Gleiche Personen, mehrere Jahrespakete, Group: Inhaltlich
        #endregion

        private void CheckStays((StatLpReport Predecessor, StatLpReport Report) data, ValidationContext<(StatLpReport Predecessor, StatLpReport Report)> ctx)
        {
            // Personen mit Aufenthalten, die auch die nachfolgende Meldung betreffen
            var p1 = data.Predecessor.Stays.Where(x => x.To == null || x.ToD > data.Predecessor.ToD).Select(x => x.PersonId);

            // Personen mit Aufenthalten, die auch die vorangehende Meldung betreffen
            var p2 = data.Report.Stays.Where(x => x.FromD < data.Report.FromD).Select(x => x.PersonId);

            // Personen deren Aufenthalt zum Ende des Meldezeitraus endet und keinen Eintrag Leavings haben
            var p3 = data.Predecessor.Stays
                .Where(x => x.ToD == data.Predecessor.ToD)
                .Where(x => !data.Predecessor.Leavings.Where(l => l.LeavingDateD == x.ToD && x.PersonId == l.PersonId).Any())
                .Select(x => x.PersonId);

            var personIds = p1.Union(p2).Union(p3).Distinct().ToArray();

            foreach (var personId in personIds)
            {
                CheckStays(data, ctx, personId);
            }
        }

        private void CheckStays((StatLpReport Predecessor, StatLpReport Report) data, ValidationContext<(StatLpReport Predecessor, StatLpReport Report)> ctx, string personId)
        {
            // die relevanten Einträge der Vorgängermeldung
            var predStays = data.Predecessor.Stays.Where(x => x.PersonId == personId).ToArray();

            // die relevanten Einträge der aktuellen Meldung
            var stays = data.Report.Stays.Where(x => x.PersonId == personId && x.FromD < data.Report.FromD).ToArray();

            if (stays.Any() && predStays.Any())
            {
                //bei einem Aufenhalt über den Meldezeitraum hinaus, ist das To des letzten Aufenthaltes des Vorgängerberichts nicht gesetzt.
                //um den Vergleich einfach zu machen, wird das To des entsprecheden ersten Aufenthalts der aktuellen Meldung ebenfalls auf null gesetzt.

                var index = stays.Length - 1;

                if (!predStays.Last().ToD.HasValue && stays[index].ToD.HasValue)
                {
                    stays[index] = new Stay(stays[index])
                    {
                        ToD = null
                    };
                }
            }

            string GetNameOfPerson() => data.Report.Persons.Where(x => x.Id == personId).Any() ? data.Report.GetPersonName(personId) : data.Predecessor.GetPersonName(personId);

            CheckStays(predStays, stays, ctx, GetNameOfPerson);

        }

        private void CheckStays(Stay[] predStays, Stay[] stays, ValidationContext<(StatLpReport Predecessor, StatLpReport Report)> ctx, Func<string> getNameOfPerson)
        {
            // verglichen werden die beiden GroupedStays 
            GroupedStay groupdPred;
            GroupedStay grouped;
            try
            {
                groupdPred = predStays.GetGroupedStays().LastOrDefault();
                grouped = stays.GetGroupedStays().FirstOrDefault();
            }
            catch (Exception e)
            {
                ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Stays)}", e.Message));
                return;
            }


            if (groupdPred == null || grouped == null || !groupdPred.Equals(grouped))
            {
                if (grouped == null)
                {
                    ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Stays)}",
                         $"Aufenthalte von '{getNameOfPerson()}' wurden letztes Jahr gemeldet. Sie fehlen in diesem Jahr: {PrintStays(groupdPred)}"));
                }
                else if (groupdPred == null)
                {
                    ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Stays)}",
                        $"Aufenthalte von '{getNameOfPerson()}' wurden dieses Jahr gemeldet. Sie fehlen im letzten Jahr:{PrintStays(grouped)}"));
                }
                else
                {
                    ctx.AddFailure(new ValidationFailure($"{nameof(StatLpReport.Stays)}",
                        $"Aufenthalte von '{getNameOfPerson()}' stimmen nicht mit der vorhergehenden Meldung überein.:{PrintStays(groupdPred)} vs. {PrintStays(grouped)}"));
                }
            }
        }


        private string PrintStays(GroupedStay stays) => $"{(string.Join(", ", stays.Stays.Select(x => $"{DisplayNameResolver.GetDisplayName($"{x.Type}")}: {x.FromD:dd.MM.yyyy}-{x.ToD:dd.MM.yyyy}")))}";
    }
}
