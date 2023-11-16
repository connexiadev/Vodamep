using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using System.Linq;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;
using Attribute = Vodamep.StatLp.Model.Attribute;

namespace Vodamep.StatLp.Validation
{

    internal class AttributeValidator : AbstractValidator<Attribute>
    {
        private static readonly DisplayNameResolver DisplayNameResolver = new DisplayNameResolver();

        public AttributeValidator(StatLpReport report)
        {
            #region Documentation
            // AreaDef: STAT
            // OrderDef: 02
            // SectionDef: Hauptmerkmal
            // StrengthDef: Fehler

            // CheckDef: Pflichtfeld
            // Fields: Hauptmerkmalstyp, Remark: Angabe vom entsprechenden Hauptmerkmal
            // Fields: Von

            // CheckDef: Erlaubte Werte
            // Fields: Hauptmerkmalstyp, Remark: Hauptmerkmale, Url: src/Vodamep/Datasets/StatLp/Attribute_type.csv
            // Fields: Von, Remark: Innerhalb des Meldungszeitraums
            // Fields: Von, Remark: Innerhalb eines Aufenthalts, Group: Inhaltlich
            #endregion

            this.RuleFor(x => x.From).SetValidator(new TimestampWithOutTimeValidator());
            this.RuleFor(x => x.From).SetValidator(new TimestampWithOutTimeValidator<Attribute, Timestamp>());

            this.RuleFor(x => x.FromD).Must(x => x >= report.FromD && x <= report.ToD)
                .WithMessage(x => Validationmessages.ReportBaseItemMustBeInReportPeriod(report.GetPersonName(x.PersonId)));

            this.RuleFor(x => x)
                .Must(x => x.ValueCase != Attribute.ValueOneofCase.None)
                .WithMessage(x => Validationmessages.ReportBaseValueMustNotBeEmptyWithString(report.GetPersonName(x.PersonId)));

            
            Stay StayOfDate(string personId, Timestamp date)
            {
                if (string.IsNullOrEmpty(personId) || date == null)
                    return null;

                return report.Stays
                    .Where(x => !string.IsNullOrEmpty(x.PersonId) && x.PersonId == personId)
                    .Where(x => (x.From == null || x.From <= date) && (x.To == null || x.To >= date))
                    .FirstOrDefault();
            }


            this.RuleFor(x => x)
                .Must(x => StayOfDate(x.PersonId, x.From) != null)
                .WithMessage(x => Validationmessages.StatLpAttributeNotWithinDate(report.GetPersonName(x.PersonId), x.FromD.ToShortDateString(), DisplayNameResolver.GetDisplayName(x.ValueCase.ToString())));
            ;
        }
    }
}
