using System;
using System.Collections.Generic;
using System.Linq;
using Vodamep.Aliases;

namespace Vodamep.StatLp.Model
{
    public static class StatLpReportExtensions
    {

        public static StatLpReport AddPerson(this StatLpReport report, Person person) => report.InvokeAndReturn(m => m.Persons.Add(person));
        public static StatLpReport AddPersons(this StatLpReport report, IEnumerable<Person> persons) => report.InvokeAndReturn(m => m.Persons.AddRange(persons));
        public static StatLpReport AddAdmission(this StatLpReport report, Admission admission) => report.InvokeAndReturn(m => m.Admissions.Add(admission));
        public static StatLpReport AddAdmissions(this StatLpReport report, IEnumerable<Admission> admissions) => report.InvokeAndReturn(m => m.Admissions.AddRange(admissions));
        public static StatLpReport AddAttribute(this StatLpReport report, Attribute attribute) => report.InvokeAndReturn(m => m.Attributes.Add(attribute));
        public static StatLpReport AddAttributes(this StatLpReport report, IEnumerable<Attribute> attributes) => report.InvokeAndReturn(m => m.Attributes.AddRange(attributes));
        public static StatLpReport AddStay(this StatLpReport report, Stay stay) => report.InvokeAndReturn(m => m.Stays.Add(stay));
        public static StatLpReport AddStays(this StatLpReport report, IEnumerable<Stay> stays) => report.InvokeAndReturn(m => m.Stays.AddRange(stays));
        public static StatLpReport AddLeaving(this StatLpReport report, Leaving leaving) => report.InvokeAndReturn(m => m.Leavings.Add(leaving));
        public static StatLpReport AddLeavings(this StatLpReport report, IEnumerable<Leaving> leavings) => report.InvokeAndReturn(m => m.Leavings.AddRange(leavings));

        public static StatLpReport AsSorted(this StatLpReport report)
        {
            var result = new StatLpReport()
            {
                Institution = report.Institution,
                From = report.From,
                To = report.To
            };

            result.Admissions.AddRange(report.Admissions.OrderBy(x => x.PersonId).ThenBy(x => x.AdmissionDate));
            result.Attributes.AddRange(report.Attributes.OrderBy(x => x.PersonId).ThenBy(x => x.From).ThenBy(x => x.ValueCase));
            result.Leavings.AddRange(report.Leavings.OrderBy(x => x.PersonId).ThenBy(x => x.LeavingDateD));
            result.Persons.AddRange(report.Persons.OrderBy(x => x.Id));
            result.Stays.AddRange(report.Stays.OrderBy(x => x.PersonId).ThenBy(x => x.From));
            result.Aliases.AddRange(result.Aliases.OrderBy(x => x.Id1));

            return result;
        }

        public static IEnumerable<GroupedStay> GetGroupedStays(this StatLpReport report, string personId, GroupedStay.SameTypeGroupMode sameTypeGroupMode = GroupedStay.SameTypeGroupMode.NotAllowed)
        {
            var result = new List<(DateTime From, DateTime To, Stay[] Stays)>();

            var stays = report.Stays.Where(x => x.PersonId == personId).OrderBy(x => x.From).ToArray();

            return stays.GetGroupedStays(sameTypeGroupMode);
        }

        public static IEnumerable<GroupedStay> GetGroupedStays(this Stay[] stays, GroupedStay.SameTypeGroupMode sameTypeGroupMode = GroupedStay.SameTypeGroupMode.NotAllowed)
        {

            #region Documentation
            // AreaDef: STAT
            // OrderDef: 03
            // SectionDef: Aufenthalt
            // StrengthDef: Fehler

            // CheckDef: Überschneidungen
            // Fields: Von/Bis/Aufnahmeart, Remark: Keine Überscheidungen bei mehreren Aufenthalten, Group: Inhaltlich

            // CheckDef: Aufeninanderfolgende
            // Fields: Von/Bis/Aufnahmeart, Remark: Unterschiedliche Aufnahmeart für aufeinanderf. Aufenthalte, Group: Inhaltlich
            #endregion


            if (stays.Length == 0)
                yield break;

            var current = new GroupedStay(stays[0].FromD, stays[0].ToD, new[] { stays[0] });

            foreach (var stay in stays.Skip(1))
            {
                var lastStay = current.Stays.Last();
                if (lastStay.PersonId != stay.PersonId)
                {
                    throw new Exception("Es können keine Aufenthalte unterschiedlicher Personen gruppiert werden!");
                }

                if (lastStay.FromD > stay.FromD)
                {
                    throw new Exception("Die Aufenthalte müssen vor dem Gruppieren nach Datum sortiert sein!");
                }

                if (current.To == null)
                {
                    throw new Exception("Die Aufenthalte dürfen sich nicht überschneiden!");
                }

                if (current.To.Value.AddDays(1) == stay.FromD)
                {
                    if (sameTypeGroupMode != GroupedStay.SameTypeGroupMode.Ignore && lastStay.Type == stay.Type)
                    {
                        if (sameTypeGroupMode == GroupedStay.SameTypeGroupMode.NotAllowed)
                        {
                            throw new Exception("Aufeinanderfolgende Aufenthalte müssen unterschiedliche Aufnahmearten haben!");
                        }

                        //zwei nacheinander folgende Aufenthalte mit gleichem Type sollen zu einem Aufenthalt vereint werden.
                        var newLastStay = new Stay(lastStay)
                        {
                            ToD = stay.ToD
                        };
                        var newStays = current.Stays.Take(current.Stays.Length - 1).Union(new[] { newLastStay }).ToArray();

                        current = new GroupedStay(current.From, stay.ToD, newStays);
                    }
                    else
                    {
                        current = new GroupedStay(current.From, stay.ToD, current.Stays.Union(new[] { stay }).ToArray());
                    }
                    continue;
                }
                else if (current.To.Value.AddDays(1) < stay.FromD)
                {
                    yield return current;
                    current = new GroupedStay(stay.FromD, stay.ToD, new[] { stay });
                    continue;
                }
                else if (current.To.Value.AddDays(1) >= stay.FromD)
                {
                    throw new Exception("Die Aufenthalte dürfen sich nicht überschneiden!");
                }

                throw new Exception("Die Aufenthalte können nicht verarbeitet werden!");
            }

            yield return current;
        }

        public static GroupedStay Clip(this GroupedStay stay, DateTime to)
        {
            if (stay == null || !stay.Stays.Any())
            {
                return null;
            }

            var clippedStays = stay.Stays.Clip(to).ToArray();

            if (!clippedStays.Any())
            {
                return null;
            }

            var result = new GroupedStay(clippedStays.First().FromD, clippedStays.Last().ToD, clippedStays);

            return result;
        }

        public static IEnumerable<Stay> Clip(this IEnumerable<Stay> stays, DateTime to)
        {
            var enumerator = stays.GetEnumerator();

            Stay current = null;

            while (enumerator.MoveNext())
            {               
                var next = new Stay(enumerator.Current);

                if (current != null)
                {
                    if (next.FromD > to)
                    {
                        current.ToD = null;
                        yield return current;
                        yield break;
                    }

                    yield return current;
                }

                current = next;

                if (current.FromD > to)
                {
                    yield break;
                }

                if (current.ToD != null && current.ToD > to)
                {
                    current.ToD = null;
                }
            }

            if (current != null)
            {
                yield return current;
            }
        }

        public static StatLpReport RemoveDoubletes(this StatLpReport report) => RemoveDoubletes(new[] { report })[0];

        public static StatLpReport[] RemoveDoubletes(this IEnumerable<StatLpReport> reports)
        {
            var map = reports.CreatePatientIdMap();

            var result = reports.Select(x => x.ApplyPersonIdMap(map)).ToArray();

            return result;
        }

        /// <summary>
        /// Erzeugt aus einer Reihe von StatLpReport eine Mappingtabelle um bei Dubletten einen eindeutigen Id zu bestimmen.
        /// Berücksichtigt werden Einträge der Alias-Liste der Reports
        /// Dubletten sind gleiche Einträge der Namensfelder und des Geburtsdatums
        /// </summary>       
        public static IDictionary<string, string> CreatePatientIdMap(this IEnumerable<StatLpReport> reports)
        {
            var aliasSystem = new AliasSystem();

            foreach (var r in reports.Where(x => x != null))
            {
                aliasSystem = aliasSystem
                    .SetAliases(r.Aliases.Where(x => x.IsAlias).Select(x => (x.Id1, x.Id2)))
                    .SetNotAliases(r.Aliases.Where(x => !x.IsAlias).Select(x => (x.Id1, x.Id2)));
            }

            var entities = reports.Where(x => x != null).SelectMany(x => x.Persons);

            aliasSystem = aliasSystem.SetAliases(entities, x => x.Id, Person.ConcatNameAndBirthday);

            var map = aliasSystem.BuildMap();

            return map;
        }

        public static IDictionary<string, string> CreatePatientIdMap(this StatLpReport report) => report != null ? (new[] { report }).CreatePatientIdMap() : new Dictionary<string, string>();


        public static StatLpReport ApplyPersonIdMap(this StatLpReport report, IDictionary<string, string> map)
        {
            report = report.Clone();

            foreach (var p in report.Persons)
            {
                if (map.TryGetValue(p.Id, out string idNew))
                {
                    //replace
                    var idOld = p.Id;

                    foreach (var admission in report.Admissions.Where(x => x.PersonId == idOld).ToArray())
                    {
                        admission.PersonId = idNew;
                    }

                    foreach (var attribute in report.Attributes.Where(x => x.PersonId == idOld).ToArray())
                    {
                        attribute.PersonId = idNew;
                    }

                    foreach (var leavings in report.Leavings.Where(x => x.PersonId == idOld).ToArray())
                    {
                        leavings.PersonId = idNew;
                    }

                    foreach (var stay in report.Stays.Where(x => x.PersonId == idOld).ToArray())
                    {
                        stay.PersonId = idNew;
                    }

                    foreach (var person in report.Persons.Where(x => x.Id == idOld).ToArray())
                    {
                        if (report.Persons.Where(x => x.Id == idNew).Any())
                        {
                            report.Persons.Remove(person);
                        }
                        else
                        {
                            person.Id = idNew;
                        }
                    }
                }
            }

            return report.AsSorted();


        }


    }
}