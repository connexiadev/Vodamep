﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Vodamep.Legacy.Model;

namespace Vodamep.Legacy.Reader
{
    public class ConnexiaReader : IReader
    {
        private readonly string _connectionstring;
        private readonly string _verein;


        public ConnexiaReader(string connectionstring, string verein)
        {
            _connectionstring = connectionstring;
            _verein = verein;
        }

        public ReadResult Read(int year, int month)
        {
            using (var connection = new SqlConnection(_connectionstring))
            {
                connection.Open();


                var from = new DateTime(year, month, 1);
                var to = from.LastDateInMonth().Date.AddHours(23).AddMinutes(59);

                var sqlLeistungen = @"select
                                      L.Pfleger,
                                      L.Adressnummer,
                                      L.Leistung,
                                      L.Anzahl,
                                      L.Datum 
                                    from 
                                         HKP_Betreuung_Leistungen L inner join Adressen A on A.Adressnummer = L.Adressnummer 
                                    where L.Verein = @verein and L.Datum between @from and @to and not (l.Leistung in (32,34))";

                var leistungen = connection.Query<LeistungDTO>(sqlLeistungen, new { verein = _verein, from = from, to = to }).ToArray();


                if (!leistungen.Any())
                    return ReadResult.Empty;

                var adressnummern = leistungen.Where(x => x.Leistung < 20).Select(x => x.Adressnummer).Distinct().ToArray();

                var sqlAdressen = @"SELECT a.Adressnummer, a.A_Name as Name_1, a.Adresse, a.Land, a.Postleitzahl, a.Geburtsdatum, a.Staatsbuergerschaft, a.Versicherung, a.Versicherungsnummer, o.Ort, a.Geschlecht
                                    FROM Adressen AS a LEFT JOIN tb_orte AS o ON o.Postleitzahl = a.Postleitzahl 
                                    where a.Adressnummer in @ids;";



                var adressen = connection.Query<AdresseDTO>(sqlAdressen, new { ids = adressnummern }).ToArray();

                //adressen = adressen.Where(x => x.Adressnummer == 37525).ToArray();

                // Vorgänger Version
                //var pflegestufenSql = @"SELECT d.Adressnummer, d.Detail
                //                        FROM Doku as d
                //                        INNER JOIN (
	            //                         SELECT d0.Adressnummer, min(d0.Datum) as Datum FROM Doku as d0
	            //                         WHERE d0.Gruppe='21' AND d0.Datum <= @to AND d0.Adressnummer in @ids 
	            //                         group by d0.Adressnummer) as dd on d.Adressnummer = dd.Adressnummer and d.Gruppe='21' and d.Datum = dd.Datum";


                var pflegestufenSql = @"Select Adressnummer, Detail, Datum
                                        from Doku
                                        where Adressnummer in @ids
                                        and Gruppe = '21'
                                        and Datum <= @to
                                        order by Datum desc";



                var pflegestufen = connection.Query<(int Adressnummer, string Wert, DateTime Datum)>(pflegestufenSql, new { to = to, ids = adressnummern }).ToArray();

                foreach (var a in adressen)
                {

                    var psList = pflegestufen.Where(x => x.Adressnummer == a.Adressnummer).OrderByDescending(x => x.Datum);

                    if (psList.Count() == 0)
                        a.Pflegestufe = (int)Hkpv.Model.CareAllowance.Unknown;
                    else
                    {

                        // Als default den, der zuletzt vor dieser Meldung gesendet wurde
                        var psFound = psList.FirstOrDefault();

                        foreach (var ps in psList)
                        {

                            // Wenn wir noch einen jüngeren Wert im Monat finden, dann nehmen wir den, 
                            // gemäß der alten Logik
                            DateTime dateTime = ps.Datum;
                            if (dateTime.Month == month &&
                                dateTime.Year == year)
                            {
                                psFound = ps;

                            }
                        }

                        a.Pflegestufe = int.Parse(psFound.Wert);
                    }
                }


                var pflegernummern = leistungen.Select(x => x.Pfleger).Distinct().ToArray();



                var sqlPfleger = @"select distinct 
                                      U.Wert as Pflegernummer,
                                      A.A_Name as Pflegername
                                    from TB_HKP_Verein_Funktion U 
                                    inner join Adressen A on A.Adressnummer = U.Adressnummer 
                                    where U.Wert in @ids";

                var pfleger = connection.Query<PflegerDTO>(sqlPfleger, new { ids = pflegernummern }).ToArray();



                var sqlAnstellung = @"select
                                      U.von Von,
                                      U.bis Bis,
                                      CONVERT(DECIMAL, REPLACE(REPLACE(V.Wert, '%', ''), ',', '.')) / 100 AS VZAE,  
                                      U.Wert Pflegernummer,
                                      V.Funktion Berufstitel
                                    from Zeit Z,
                                         TB_HKP_Verein_Funktion U inner join TB_HKP_Verein_Funktion V 
                                                                             on V.Adressnummer=U.Adressnummer 
                                                                             and V.von=U.von 
                                                                             and coalesce(V.bis,V.von) = coalesce(U.bis,V.von)
                                                                  inner join Adressen A on A.Adressnummer = U.Adressnummer 
                                    where U.Funktion = 'PFL' 
                                      and not V.Funktion = 'PFL' 
                                      AND U.Verein = @verein
                                      AND U.Wert IN @ids";

                string joined = string.Join(",", pflegernummern);

                var anstellungenGesamt = connection.Query<AnstellungDTO>(sqlAnstellung, new { verein = _verein, ids = pflegernummern }).ToArray();
                List<AnstellungDTO> anstellungen = new List<AnstellungDTO>();


                foreach (PflegerDTO p in pfleger)
                {
                    // Liste mit Anstellungen für den Meldungszeitraum
                    List<AnstellungDTO> anstellungPflegerList = anstellungenGesamt.Where(x => x.Pflegernummer == p.Pflegernummer)
                                                                            .OrderBy(x => x.Von.GetValueOrDefault(DateTime.MinValue)).ToList();

                    List<AnstellungDTO> anstellungPflegerZeitraumList = anstellungPflegerList.Where(x => (x.Bis.GetValueOrDefault(DateTime.MaxValue) >= from  && 
                                                                                                          x.Von.GetValueOrDefault(DateTime.MinValue) <= to))
                                                                                             .OrderBy(x => x.Von.GetValueOrDefault(DateTime.MinValue)).ToList();

                    anstellungen.AddRange(anstellungPflegerZeitraumList);



                    // Aktueller Berufstitel
                    AnstellungDTO anstellung = anstellungPflegerZeitraumList.LastOrDefault();

                    if (anstellung == null)
                    {
                        anstellung = anstellungPflegerList.FirstOrDefault();
                        Debug.WriteLine(String.Format("Pfleger {0} Verein {1} Monat {2}", p.Pflegernummer, _verein, from.ToShortDateString()));
                    }


                    string aktuellerTitel = anstellung?.Berufstitel;

                    if (aktuellerTitel == null)
                        aktuellerTitel = "";

                    p.Berufstitel = aktuellerTitel;

                }




                var sqlVerein = @"select distinct
                                  V.Verein as Vereinsnummer, 
                                  coalesce(A.A_Name,'') + ' ' + coalesce(A.Name_1,'') AS Bezeichnung 
                                from TB_HKP_Verein V
                                inner join Adressen A on A.Adressnummer = V.Adressnummer
                                where V.Verein = @verein";

                var verein = connection.QueryFirst<VereinDTO>(sqlVerein, new { verein = _verein });

                return new ReadResult() { A = adressen, P = pfleger, L = leistungen, V = verein, S = anstellungen.ToArray() };
            }
        }


        public static VereinDTO[] GetVereine(string connectionstring, int year, string vereinsnummer)
        {
            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();

                var sqlVereine = @"select distinct
                                      V.Verein AS Vereinsnummer, 
                                      coalesce(A.A_Name,'') + ' ' + coalesce(A.Name_1,'') AS Bezeichnung 
                                    from HKP_Betreuung_Leistungen L inner join TB_HKP_Verein V on V.Verein = L.Verein 
                                                                    inner join Adressen A on A.Adressnummer = V.Adressnummer 
                                    where L.Datum between @from and @to ";

                if (!String.IsNullOrWhiteSpace(vereinsnummer))
                {
                    sqlVereine += "and V.Verein = @verein";
                }



                var vereine = connection.Query<VereinDTO>(sqlVereine,
                    new
                    {
                        from = new DateTime(year, 1, 1),
                        to = new DateTime(year, 12, 31),
                        verein = vereinsnummer

                    }).ToArray();

                return vereine;
            }
        }
    }
}
