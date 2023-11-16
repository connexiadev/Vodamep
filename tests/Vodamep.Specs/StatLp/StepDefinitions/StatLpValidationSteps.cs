using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Vodamep.Data.Dummy;
using Vodamep.StatLp.Model;
using Vodamep.StatLp.Validation;
using Attribute = Vodamep.StatLp.Model.Attribute;
using Enum = System.Enum;

namespace Vodamep.Specs.StatLp.StepDefinitions
{

    [Binding]
    public class StatLpValidationSteps
    {
        private readonly ReportContext _context;

        public StatLpValidationSteps(ReportContext context)
        {
            if (context.Report == null)
            {
                InitContext(context);
            }

            _context = context;
        }

        private void InitContext(ReportContext context)
        {
            context.GetPropertiesByType = GetPropertiesByType;

            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);

            var r = StatLpDataGenerator.Instance.CreateStatLpReport("0001", 2021, 1);

            context.Report = r;
        }

        private IEnumerable<IMessage> GetPropertiesByType(string type)
        {
            return type switch
            {
                nameof(Person) => this.Report.Persons,
                nameof(Admission) => this.Report.Admissions,
                nameof(Leaving) => this.Report.Leavings,
                nameof(Attribute) => this.Report.Attributes,
                nameof(Stay) => this.Report.Stays,
                nameof(Institution) => new[] { this.Report.Institution },
                _ => Array.Empty<IMessage>(),
            };
        }

        public StatLpReport Report => _context.Report as StatLpReport;

        [Given(@"es ist ein 'StatLpReport'")]
        public void GivenItIsAHkpvReport()
        {

        }

        [Given(@"die Auflistungs Eigenschaft von Admission mit dem Auflistungstyp '(\w*)' ist auf '(.*)' gesetzt")]
        public void GivenTheEnumLIstPropertyFromAdmissionIsSetTo(string type, string value)
        {
            //todo geht bestimmt eleeganter
            switch (type)
            {
                case "PersonalChanges":
                    this.SetPersonalChanges(value);
                    break;

                case "SocialChanges":
                    this.SetSocialChanges(value);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Given("mit den Stammdaten der StatLp-Person mit Id '1' gibt es einen weiteren Aufenthalt als Person mit Id '1X'")]
        public void GivenADuplicate()
        {

            var p1 = this.Report.Persons.Where(x => x.Id == "1").First();
            var p2 = new Person(p1)
            {
                Id = $"1X"
            };

            this.Report.Persons.Add(p2);

            var from2 = this.Report.ToD.Date.AddDays(-10);

            this.Report.AddAdmission(new Admission(this.Report.Admissions.Where(x => x.PersonId == p1.Id).First())
            {
                PersonId = p2.Id,
                AdmissionDateD = from2
            });

            foreach (var att in this.Report.Attributes.Where(x => x.PersonId == p1.Id))
            {
                this.Report.Attributes.Add(new Attribute(att)
                {
                    PersonId = p2.Id,
                    FromD = from2
                });
            }

            this.Report.AddStay(new Stay
            {
                PersonId = p2.Id,
                FromD = from2,
                Type = AdmissionType.ContinuousAt
            });
        }

        [Given("es gibt einen Alias '1'='1X'")]
        public void GivenAnAlias()
        {
            this.Report.Aliases.Add(new Alias
            {
                Id1 = "1",
                Id2 = "1X",
                IsAlias = true
            });
        }

        private void SetPersonalChanges(string value)
        {
            this.Report.Admissions.First().PersonalChanges.Clear();

            if (value.Contains(','))
            {
                var personalChange = value.Split(',').Select(x => (PersonalChange)Enum.Parse(typeof(PersonalChange), x));
                this.Report.Admissions[0].PersonalChanges.AddRange(personalChange);
            }
            else if (Enum.TryParse(value, out PersonalChange activityType))
            {
                this.Report.Admissions[0].PersonalChanges.Add(activityType);
            }
            else if (value == "")
            {
                //nothing do do, already emptied
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void SetSocialChanges(string value)
        {
            this.Report.Admissions.First().SocialChanges.Clear();

            if (value.Contains(','))
            {
                var activityTypes = value.Split(',').Select(x => (SocialChange)Enum.Parse(typeof(SocialChange), x));
                this.Report.Admissions[0].SocialChanges.AddRange(activityTypes);
            }
            else if (Enum.TryParse(value, out SocialChange socialChange))
            {
                this.Report.Admissions[0].SocialChanges.Add(socialChange);
            }
            else if (value == "")
            {
                //nothing do do, already emptied
            }
            else
            {
                throw new NotImplementedException();
            }
        }



        [Given(@"der Id einer Person ist nicht eindeutig")]
        public void GivenPersonIdNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Id = p0.Id;
            p.Id = p0.Id;
        }


        [Given(@"es gibt eine weitere Person")]
        public void GivenThereIsAnotherPerson()
        {
            var p = this.Report.AddDummyPerson(2);
        }

        [Given(@"Bis ist vor Von bei einem Stay")]
        public void GivenToIsBeforeFromAtFirstStay()
        {
            var stay = this.Report.Stays[0];

            stay.From = stay.To.AsDate().AddDays(2).AsTimestamp();
        }

        [Given(@"das Attribut '(.*)' fehlt")]
        public void GivenAttributeIsMissing(string attributeType)
        {
            var type = Enum.Parse<Attribute.ValueOneofCase>(attributeType);

            this.Report.Attributes.Remove(this.Report.Attributes.FirstOrDefault(x => x.ValueCase == type));
        }

        [Given(@"das Attribut mit dem  Typ '(.*)' ist auf den Wert '(.*)' gesetzt")]
        public void GivenAttributeValueIsSet(string attributeType, string value)
        {
            var type = Enum.Parse<Attribute.ValueOneofCase>(attributeType);

            var attribute = this.Report.Attributes.First(x => x.ValueCase == type);

            attribute.SetValue(attributeType, value);
        }

        [Given(@"es gibt am '(.*)' ein zusätzliches Attribut vom Typ '(.*)' und dem Wert '(.*)'")]
        public void GivenThereIsOneAdditionalAttribute(string date, string attributeType, string value)
        {
            var attribute = new Attribute()
            {
                FromD = DateTime.Parse(date, new CultureInfo("de-DE")),
                PersonId = this.Report.Persons[0].Id
            };

            attribute.SetValue(attributeType, value);

            this.Report.Attributes.Add(attribute);
        }


        [Given(@"die erste Aufnahme startet am '(.+)', dauert (\d+) Tage und ist eine '(\w+)'")]
        public void GivenTheFirstStayWithDuration(string date, int days, string type)
        {
            this.Report.Stays.Clear();
            this.Report.Leavings.Clear();
            this.Report.Admissions.Clear();
            this.Report.Attributes.Clear();

            this.GivenAnOtherStayWithDuration(date, days, type);

        }

        [Given(@"eine weitere Aufnahme startet am '(.+)', dauert (\d+) Tage und ist eine '(\w+)'")]
        public void GivenAnOtherStayWithDuration(string date, int days, string type)
        {

            var personId = this.Report.Persons[0].Id;
            var from = DateTime.Parse(date, new CultureInfo("de-DE"));

            var stay = new Stay
            {
                PersonId = personId,
                FromD = from,
                ToD = days >= 0 ? from.AddDays(days) : null,
                Type = Enum.Parse<AdmissionType>(type)
            };

            if (stay.ToD > this.Report.ToD)
            {
                stay.ToD = null;
            }

            this.Report.Stays.Add(stay);

            this.Report.Admissions.Add(StatLpDataGenerator.Instance.CreateAdmission(personId, from));

            this.Report.Attributes.Add(StatLpDataGenerator.Instance.CreateAttributes(personId, from >= this.Report.FromD ? from : this.Report.FromD));

            // ggf. das Leaving des direkt vorngehenden Aufenthaltes entfernen
            var removeLeaving = this.Report.Leavings.Where(x => x.PersonId == personId && x.LeavingDateD == from.AddDays(-1)).FirstOrDefault();
            if (removeLeaving != null)
            {
                this.Report.Leavings.Remove(removeLeaving);
            }

            // ggf eine Leaving erzeugen
            if (stay.To != null && stay.From >= this.Report.From)
            {
                this.Report.Leavings.Add(StatLpDataGenerator.Instance.CreateLeaving(personId, stay.ToD.Value));
            }

        }

        [Given(@"die erste Aufnahme startet am '(.+)' und ist eine '(\w+)'")]
        public void GivenTheFirstStay(string date, string type) => this.GivenTheFirstStayWithDuration(date, -1, type);

        [Given(@"es gibt eine weitere Aufnahme '(\w+)', die (\d+) Tage dauert")]
        public void GivenAnotherStay(string type, int days) => this.GivenAnotherStayWithGap(type, days, 0);


        [Given(@"es gibt eine weitere Aufnahme '(\w+)', die (\d+) Tage dauert, dazwischen liegen (-?\d+) Tage")]
        public void GivenAnotherStayWithGap(string type, int days, int gap)
        {
            var personId = this.Report.Persons[0].Id;
            var from = this.Report.Stays.Where(x => x.PersonId == personId && x.To != null).Select(x => x.ToD.Value).Max().AddDays(1).AddDays(gap);

            var stay = new Stay
            {
                PersonId = personId,
                FromD = from,
                ToD = from.AddDays(days),
                Type = Enum.Parse<AdmissionType>(type)
            };

            if (stay.ToD > Report.ToD)
            {
                stay.ToD = null;
            }

            this.Report.Stays.Add(stay);
        }

        [Given("es gibt eine Entlassungsmeldung mehrfach")]
        public void GivenMultipleLeavings()
        {
            if (this.Report.Leavings.Any())
            {
                this.Report.Leavings.Add(new Leaving(this.Report.Leavings[0]));
            }
        }
    }
}
