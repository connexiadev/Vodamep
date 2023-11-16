using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Vodamep.Data.Dummy;
using Vodamep.StatLp.Model;
using Vodamep.StatLp.Validation;
using Attribute = Vodamep.StatLp.Model.Attribute;

namespace Vodamep.Specs.StatLp.StepDefinitions
{
    [Binding]
    public class StatLpUpdateValidationSteps
    {
        private readonly ReportContext _context;

        public StatLpUpdateValidationSteps(ReportContext context)
        {
            if (context.Report == null)
            {
                InitContext(context);
            }

            _context = context;
        }

        [Given("es gibt eine aktualisierte StatLp-Datenmeldungen")]
        public void GivenUpdateReport()
        {

        }

        private void InitContext(ReportContext context)
        {
            context.GetPropertiesByType = this.GetPropertiesByType;            
            context.Validate = () =>  this.Report.Validate((StatLpReport)context.PrecedingReport);

            var loc = new DisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);

            var r1 = StatLpDataGenerator.Instance.CreateStatLpReport("0001", 2021, 1);

            r1.Attributes.Where(x => x.ValueCase == Attribute.ValueOneofCase.CareAllowance).FirstOrDefault().CareAllowance = CareAllowance.L3;
            r1.Attributes.Where(x => x.ValueCase == Attribute.ValueOneofCase.CareAllowanceArge).FirstOrDefault().CareAllowanceArge = CareAllowanceArge.L3Ar;

            var r2 = new StatLpReport(r1);

            context.PrecedingReport = r1;
            context.Report = r2;

        }

        public StatLpReport Report => _context.Report as StatLpReport;

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
