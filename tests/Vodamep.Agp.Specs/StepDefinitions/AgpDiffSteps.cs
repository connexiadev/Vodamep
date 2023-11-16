using FluentValidation;
using System.Globalization;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using Vodamep.Agp.Model;
using Vodamep.Agp.Validation;
using Vodamep.Data.Dummy;
using Vodamep.ReportBase;
using Xunit;

namespace Vodamep.Specs.Agp.StepDefinitions
{

    [Binding]
    public class AgpDiffSteps
    {

        public AgpDiffSteps()
        {
            var loc = new AgpDisplayNameResolver();
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => loc.GetDisplayName(memberInfo?.Name);
        }

        public AgpReport Report1 { get; private set; }

        public AgpReport Report2 { get; private set; }

        [BeforeScenario()]
        public void BeforeScenario()
        {
            this.Report1 = AgpDataGenerator.Instance.CreateAgpReport(string.Empty, null, null, 1, 1, true);

            this.Report1.Persons.First().Id = "1";
            this.Report1.Activities.First().PersonId = "1";

            this.Report2 = this.Report1.Clone();
        }


        [Given(@"Ein Property eines Reports hat sich geändert.")]
        public void GivenAllPropertiesOfTheSecondReportHaveChanged()
        {
            this.Report1.Persons.First().City = "Test";
        }

        [Then(@"enthält das Ergebnis '(.*)' Objekte\(e\)")]
        public void ThenTheResultContainsNrOfElements(int nrOfResults)
        {
            var diffResult = this.Report1.DiffList(this.Report2);

            Assert.Equal(nrOfResults, diffResult.Count);
        }

        [Then(@"ein Element besitzt den Status '(.*?)' mit der Id '(.*?)', dem Wert1 '(.*?)' und dem Wert2 '(.*?)'")]
        public void ThenTheResultContainsEntry(Difference difference, DifferenceIdType differenceId, string value1, string value2)
        {
            if (string.IsNullOrWhiteSpace(value1))
                value1 = null;

            if (string.IsNullOrWhiteSpace(value2))
                value2 = null;

            var diffResults = this.Report1.DiffList(this.Report2);

            var filteredDiffResults = diffResults.Where(x => x.Difference == difference &&
                                                             x.DifferenceId == differenceId);

            DiffObject diffResult = null;

            foreach (var dr in filteredDiffResults)
            {
                var value1AString = dr.Value1 is double doubleValue1
                    ? doubleValue1.ToString(CultureInfo.InvariantCulture)
                    : dr.Value1?.ToString();

                var value2AString = dr.Value2 is double doubleValue2
                    ? doubleValue2.ToString(CultureInfo.InvariantCulture)
                    : dr.Value2?.ToString();

                if (value1 == value1AString && value2 == value2AString)
                {
                    diffResult = dr;
                    break;
                }
            }

            Assert.NotNull(diffResult);

            //var value1AString = "";
            //if (diffResult.Value1 is double doubleValue1)
            //    value1AString = doubleValue1.ToString(CultureInfo.InvariantCulture);
            //else
            //    value1AString = diffResult.Value1?.ToString();

            //var value1AString = diffResult.Value1 is double doubleValue1
            //    ? doubleValue1.ToString(CultureInfo.InvariantCulture)
            //    : diffResult.Value1?.ToString();

            //var value2AString = "";
            //if (diffResult.Value2 is double doubleValue2)
            //    value2AString = doubleValue2.ToString(CultureInfo.InvariantCulture);
            //else
            //    value2AString = diffResult.Value2?.ToString();

            //Assert.Equal(difference, diffResult.Difference);
            //Assert.Equal(differenceId, diffResult.DifferenceId);
            //Assert.Equal(value1, value1AString);
            //Assert.Equal(value2, value2AString);
        }

    }

}
