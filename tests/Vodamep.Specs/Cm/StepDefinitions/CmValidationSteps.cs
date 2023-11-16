using FluentValidation;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Vodamep.Cm.Model;
using Vodamep.Cm.Validation;
using Vodamep.Data.Dummy;

namespace Vodamep.Specs.Cm.StepDefinitions
{

    [Binding]
    public class CmValidationSteps
    {

        private readonly ReportContext _context;

        public CmValidationSteps(ReportContext context)
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

            var r = CmDataGenerator.Instance.CreateCmReport("", 2021, 2, 1, 1, false);

            context.Report = r;
        }

        private IEnumerable<IMessage> GetPropertiesByType(string type)
        {
            return type switch
            {
                nameof(Person) => this.Report.Persons,
                nameof(Institution) => new[] { this.Report.Institution },
                nameof(Activity) => this.Report.Activities,
                nameof(ClientActivity) => this.Report.ClientActivities,
                _ => Array.Empty<IMessage>(),
            };
        }

        public CmReport Report => _context.Report as CmReport;

        [Given(@"es ist ein 'CmReport'")]
        public void GivenItIsACmReport()
        {

        }

        [Given(@"der Id einer Cm-Person ist nicht eindeutig")]
        public void GivenPersonIdNotUnique()
        {
            var p0 = this.Report.Persons[0];

            var p = this.Report.AddDummyPerson();

            p.Id = p0.Id;
            p.Id = p0.Id;
        }

        [Given(@"für einen Cm-Klient gibt es mehrfache Leistungen")]
        public void GivenMultipleActivitiesForOneClient()
        {
            this.Report.AddDummyActivity();
        }



    }
}
