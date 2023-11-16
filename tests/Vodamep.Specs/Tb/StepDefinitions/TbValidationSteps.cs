using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Vodamep.Data.Dummy;
using Vodamep.Tb.Model;
using Vodamep.Tb.Validation;

namespace Vodamep.Specs.Tb.StepDefinitions
{

    [Binding]
    public class TbValidationSteps
    {

        private readonly ReportContext _context;

        public TbValidationSteps(ReportContext context)
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

            var date = DateTime.Today.AddMonths(-1);
            var r = TbDataGenerator.Instance.CreateTbReport("", date.Year, date.Month, 1, 1, false);

            context.Report = r;
        }

        private IEnumerable<IMessage> GetPropertiesByType(string type)
        {
            return type switch
            {
                nameof(Person) => this.Report.Persons,
                nameof(Institution) => new[] { this.Report.Institution },
                nameof(Activity) => this.Report.Activities,
                _ => Array.Empty<IMessage>(),
            };
        }

        public TbReport Report => _context.Report as TbReport;

        [Given(@"es ist ein 'TbReport'")]
        public void GivenItIsATbReport()
        {

        }

        [Given(@"der Id einer Tb-Person ist nicht eindeutig")]
        public void GivenPersonIdNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Id = p0.Id;
            p.Id = p0.Id;
        }

        [Given(@"für einen Tb-Klient gibt es mehrfache Leistungen")]
        public void GivenMultipleActivitiesForOneClient()
        {
            this.Report.AddDummyActivity();
        }
    }
}
