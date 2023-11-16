using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using TechTalk.SpecFlow;
using Vodamep.Data.Dummy;
using Vodamep.StatLp.Model;
using Vodamep.StatLp.Validation;
using Attribute = Vodamep.StatLp.Model.Attribute;

namespace Vodamep.Specs.StatLp.StepDefinitions
{
    [Binding]
    public class StatLpAdjacentValidationSteps
    {
        private readonly ReportContext _context;

        public StatLpAdjacentValidationSteps(ReportContext context)
        {
            if (context.Report == null)
            {
                InitContext(context);
            }

            _context = context;
        }

        private void InitContext(ReportContext context)
        {
            context.GetPropertiesByType = this.GetPropertiesByType;

            context.Validate = () => context.Report.Validate(context.PrecedingReport);

            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);

            var r1 = StatLpDataGenerator.Instance.CreateStatLpReport("0001", 2020, 1);
            r1.Admissions[0].Gender = Gender.FemaleGe;
            r1.Persons[0].BirthdayD = new DateTime(1955, 1, 1);
            r1.Stays[0].To = null;
            r1.Leavings.Clear();

            var r2 = new StatLpReport(r1)
            {
                FromD = r1.FromD.AddYears(1),
                ToD = r1.ToD.AddYears(1)
            };

            foreach (var attribut in r2.Attributes)
            {
                attribut.FromD = r2.FromD;
            }

            context.PrecedingReport = r1;
            context.Report = r2;

        }

        public StatLpReport Report => _context.Report as StatLpReport;

        [Given("es gibt zwei aneinander grenzende StatLp-Datenmeldungen")]
        public void GivenAdjacentReports()
        {

        }
        
        [Given(@"zum Jahresende wechselt die AufnahmeArt von '(\w+)' zu '(\w+)'")]
        public void GivenChangeOfStayType(string type1, string type2)
        {
            var r1 = (StatLpReport)_context.PrecedingReport;
            var r2 = (StatLpReport)_context.Report;
                        
            r1.Stays[0].ToD = r1.ToD;
            r1.Stays[0].Type = Enum.Parse<AdmissionType>(type1);

            r2.Stays[0].ToD = r1.ToD;
            r2.Stays[0].Type = Enum.Parse<AdmissionType>(type1);

            r2.Stays.Add(new Stay
            {
                FromD = r2.FromD,
                Type = Enum.Parse<AdmissionType>(type2)
            });

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
    }
}
