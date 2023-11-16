using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Vodamep.Data.Dummy;
using Vodamep.Mohi.Model;
using Vodamep.Mohi.Validation;

namespace Vodamep.Specs.Mohi.StepDefinitions
{

    [Binding]
    public class MohiValidationSteps
    {
        private readonly ReportContext _context;

        public MohiValidationSteps(ReportContext context)
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

            var date = new DateTime(2021, 05, 01);
            var r = MohiDataGenerator.Instance.CreateMohiReport("", date.Year, date.Month, 1, 1, false);

            context.Report = r;
        }

        private IEnumerable<IMessage> GetPropertiesByType(string type)
        {
            return type switch
            {
                nameof(Person) => this.Report.Persons,
                nameof(Institution) => new[] { this.Report.Institution },
                nameof(Activity) => this.Report.Activities,
                _ => Array.Empty<IMessage>()
            };
        }

        public MohiReport Report => _context.Report as MohiReport;

        [Given(@"es ist ein 'MohiReport'")]
        public void GivenItIsAMohiReport()
        {

        }


        [Given(@"der Id einer Mohi-Person ist nicht eindeutig")]
        public void GivenPersonIdNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Id = p0.Id;
            p.Id = p0.Id;
        }


        [Given(@"für einen Mohi-Klient gibt es mehrfache Leistungen")]
        public void GivenMultipleActivitiesForOneClient()
        {
            this.Report.AddDummyActivity();
        }
    }
}
