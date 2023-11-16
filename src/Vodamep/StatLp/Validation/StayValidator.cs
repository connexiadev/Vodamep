using FluentValidation;
using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Vodamep.StatLp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.StatLp.Validation
{
    internal class StayValidator : AbstractValidator<Stay>
    {
        private static readonly DisplayNameResolver displayNameResolver = new DisplayNameResolver();

        public StayValidator(StatLpReport report)
        {
            this.RuleFor(x => x.From).NotEmpty();

            this.RuleFor(x => x.From).SetValidator(new TimestampWithOutTimeValidator<Stay, Timestamp>());
            this.RuleFor(x => x.To).SetValidator(new TimestampWithOutTimeValidator<Stay, Timestamp>());

            this.RuleFor(x => x.To.AsDate()).GreaterThanOrEqualTo(x => x.From.AsDate())
                .Unless(x => x.From == null || x.To == null).WithMessage(Validationmessages.FromMustBeBeforeTo);

            this.RuleFor(x => x.PersonId)
                .Must((stay, personId) =>
                {
                    return report.Persons.Any(y => y.Id == personId);
                })
                .WithMessage(x => Validationmessages.PersonIsNotAvailable(report.GetPersonName(x.PersonId)));


            // Undefinierte Aufnahmeart
            this.RuleFor(x => new { x.Type })
                .Must((a) =>
                {
                    if (a.Type == AdmissionType.UndefinedAt)
                    {
                        return false;
                    }

                    return true;
                })
                .WithMessage(x => Validationmessages.StatLpAttributeInvalidAdmissionType(report.GetPersonName(x.PersonId), $"{displayNameResolver.GetDisplayName(x.Type.ToString())}", x.FromD.ToShortDateString()));


            // Ungültige Aufnahmeart 'Probe'
            this.RuleFor(x => new { x.Type, x.FromD })
                .Must((a) =>
                {
                    if (a.Type == AdmissionType.TrialAt &&
                        a.FromD > new DateTime(2014, 08, 01))
                    {
                        return false;
                    }

                    return true;
                })
                .WithMessage(x => Validationmessages.StatLpAttributeInvalidAdmissionType(report.GetPersonName(x.PersonId), $"{displayNameResolver.GetDisplayName(x.Type.ToString())}", x.FromD.ToShortDateString()));


            // Ungültige Aufnahmeart 'Krisenintervention'
            this.RuleFor(x => new { x.Type, x.FromD })
                .Must((a) =>
                {
                    if (a.Type == AdmissionType.CrisisInterventionAt &&
                       a.FromD > new DateTime(2019, 11, 30))
                    {
                        return false;
                    }

                    return true;
                })
                .WithMessage(x => Validationmessages.StatLpAttributeInvalidAdmissionType(report.GetPersonName(x.PersonId), $"{displayNameResolver.GetDisplayName(x.Type.ToString())}", x.FromD.ToShortDateString()));


            // Ungültige Aufnahmeart 'Übergangspflege'
            this.RuleFor(x => new { x.Type, x.FromD })
                .Must((a) =>
                {
                    if (a.Type == AdmissionType.TransitionalAt &&
                       a.FromD > new DateTime(2022, 1, 1))
                    {
                        return false;
                    }

                    return true;
                })
                .WithMessage(x => Validationmessages.StatLpAttributeInvalidAdmissionType(report.GetPersonName(x.PersonId), $"{displayNameResolver.GetDisplayName(x.Type.ToString())}", x.FromD.ToShortDateString()));


        }
    }
}
