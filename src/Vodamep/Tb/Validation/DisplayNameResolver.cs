using System.Collections.Generic;
using Vodamep.Tb.Model;

namespace Vodamep.Tb.Validation
{
    internal class DisplayNameResolver
    {
        private readonly IDictionary<string, string> _dict = new SortedDictionary<string, string>();

        public DisplayNameResolver()
        {
            Init();
        }

        private void Init()
        {
            _dict.Add(nameof(TbReport.To), "Bis");
            _dict.Add(nameof(TbReport.ToD), "Bis");

            _dict.Add(nameof(TbReport.From), "Von");
            _dict.Add(nameof(TbReport.FromD), "Von");

            _dict.Add(nameof(TbReport.Institution), "Einrichtung");

            _dict.Add(nameof(Person), "Klient");
            _dict.Add(nameof(Person.GivenName), "Vorname");
            _dict.Add(nameof(Person.FamilyName), "Familienname");
            _dict.Add(nameof(Person.Birthday), "Geburtsdatum");
            _dict.Add(nameof(Person.BirthdayD), "Geburtsdatum");
            _dict.Add(nameof(Person.AdmissionType), "Aufnahmeart");

            _dict.Add(nameof(Person.CareAllowance), "Pflegegeld");
            _dict.Add(nameof(Person.Postcode), "PLZ");
            _dict.Add(nameof(Person.City), "Ort");
            _dict.Add(nameof(Person.Gender), "Geschlecht");
            _dict.Add(nameof(Person.Nationality), "Staatsbürgerschaft");
            _dict.Add(nameof(Person.MainAttendanceRelation), "Verwandtschaftsverhältnis Hauptbetreuungspers.");
            _dict.Add(nameof(Person.MainAttendanceCloseness), "Räumliche Nähe Hauptbetreuungsperson");
        }

        public string GetDisplayName(string name)
        {            
            if (!string.IsNullOrEmpty(name) && _dict.TryGetValue(name, out string value))
                return value;

            return name;
        }

    }
}
