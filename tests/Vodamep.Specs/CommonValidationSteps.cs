using FluentValidation;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using Xunit;

namespace Vodamep.Specs
{
    

    [Binding]    
    public class CommonValidationSteps
    {
        private readonly ReportContext _context;

        public CommonValidationSteps(ReportContext context)
        {
            _context = context;
        }

        [Given(@"die Eigenschaft '(\w*)' von '(\w*)' ist nicht gesetzt")]
        public void GivenThePropertyIsDefault(string name, string type)
        {
            if (type == _context.Report.GetType().Name)
                _context.ReportM.SetDefault(name);
            else
            {
                foreach (var a in this._context.GetPropertiesByType(type))
                    a.SetDefault(name);
            }
        }

        [Given(@"die Eigenschaft '(\w*)' von '(\w*)' ist auf '(.*)' gesetzt")]
        public void GivenThePropertyIsSetTo(string name, string type, string value)
        {
            if (type == _context.Report.GetType().Name)
                _context.ReportM.SetValue(name, value);
            else
            {
                foreach (var a in this._context.GetPropertiesByType(type))
                    a.SetValue(name, value);
            }
        }

        [Given(@"die Datums-Eigenschaft '(\w*)' von '(\w*)' hat eine Uhrzeit gesetzt")]
        public void GivenThePropertyHasATime(string name, string type)
        {
            IMessage m;
            if (type == _context.Report.GetType().Name)
                m = _context.ReportM;
            else
            {
                m = this._context.GetPropertiesByType(type).FirstOrDefault();
            }
            if (m != null)
            {
                var field = m.GetField(name);
                var ts = (field.Accessor.GetValue(m) as Timestamp) ?? this._context.Report.From;

                ts.Seconds = ts.Seconds + 60 * 60;
                field.Accessor.SetValue(m, ts);
            }
        }


        [Given(@"eine Meldung ist korrekt befüllt")]
        public void GivenAValidReport()
        {
            // nichts zu tun
        }

        [Given(@"alle Listen sind leer")]
        public void GivenAllListsAreEmpty()
        {

            foreach (var field in _context.ReportM.Descriptor.Fields.InFieldNumberOrder()
                 .Where(x => x.IsRepeated))
            {
                if (field.Accessor.GetValue(_context.ReportM) is IList list)
                {
                    list.Clear();
                }
            }
        }



        [Given(@"die Liste '(.*)' ist leer")]
        public void GivenTheListIsEmpty(string value)
        {
            foreach (var type in value.Split(","))
            {
                var field = _context.ReportM.Descriptor.Fields.InFieldNumberOrder()
                    .Where(x => x.IsRepeated)
                    .Where(x => x.MessageType.Name == type)
                    .FirstOrDefault();

                var l = field?.Accessor.GetValue(_context.ReportM) as IList;
                l?.Clear();
            }
        }


        [Then(@"*enthält (das Validierungsergebnis )?keine Fehler")]
        public void ThenTheResultContainsNoErrors(string dummy)
        {
            Assert.True(this._context.Result.IsValid);
            Assert.Empty(this._context.Result.Errors.Where(x => x.Severity == Severity.Error));
        }

        [Then(@"*enthält (das Validierungsergebnis )?keine Warnungen")]
        public void ThenTheResultContainsNoWarnings(string dummy)
        {
            Assert.Empty(this._context.Result.Errors.Where(x => x.Severity == Severity.Warning));
        }

        [Then(@"*enthält (das Validierungsergebnis )?genau einen Fehler")]
        public void ThenTheResultContainsOneError(object test)
        {
            Assert.False(this._context.Result.IsValid);
            Assert.Single(this._context.Result.Errors.Where(x => x.Severity == Severity.Error).Select(x => x.ErrorMessage).Distinct());
        }

        [Then(@"die Fehlermeldung lautet: '(.*)'")]
        public void ThenTheResultContainsJust(string message)
        {
            var pattern = new Regex(message, RegexOptions.IgnoreCase);

            Assert.Single(this._context.Result.Errors.Where(x => x.Severity == Severity.Error && pattern.IsMatch(x.ErrorMessage))
                .Select(e => e.ErrorMessage)
                .Distinct());
        }

        [Then(@"enthält das Validierungsergebnis den Fehler '(.*)'")]
        public void ThenTheResultContainsAnError(string message)
        {
            var pattern = new Regex(message, RegexOptions.IgnoreCase);

            if (message.Contains("1"))
            {
            }

            var result = this._context.Result.Errors.Where(x => x.Severity == Severity.Error && pattern.IsMatch(x.ErrorMessage));
            Assert.NotEmpty(result);
        }

        [Then(@"enthält das escapte Validierungsergebnis den Fehler '(.*)'")]
        public void ThenTheResultContainsAnErrorRegex(string message)
        {
            var pattern = new Regex(Regex.Escape(message), RegexOptions.IgnoreCase);

            Assert.NotEmpty(this._context.Result.Errors.Where(x => x.Severity == Severity.Error && pattern.IsMatch(x.ErrorMessage)));
        }

        [Then(@"enthält das Validierungsergebnis nicht den Fehler '(.*)'")]
        public void ThenTheResultDoesNotContainsEntry(string message)
        {
            var pattern = new Regex(message, RegexOptions.IgnoreCase);

            Assert.Empty(this._context.Result.Errors.Where(x => pattern.IsMatch(x.ErrorMessage)));
        }

        [Then(@"enthält das Validierungsergebnis die Warnung '(.*)'")]
        public void ThenTheResultContainsAnWarning(string message)
        {
            var pattern = new Regex(message, RegexOptions.IgnoreCase);

            Assert.NotEmpty(this._context.Result.Errors.Where(x => x.Severity == Severity.Warning && pattern.IsMatch(x.ErrorMessage)));
        }
    }
}
