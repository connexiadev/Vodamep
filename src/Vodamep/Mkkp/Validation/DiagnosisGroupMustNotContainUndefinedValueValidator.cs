using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Mkkp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Mkkp.Validation
{
    internal class DiagnosisGroupMustNotContainUndefinedValueValidator : AbstractValidator<Person>
    {
        public DiagnosisGroupMustNotContainUndefinedValueValidator()
        {
            #region Documentation
            // AreaDef: MKKP
            // OrderDef: 03
            // SectionDef: Klient
            // StrengthDef: Fehler

            // CheckDef: Muss Feld
            // Fields: Diagnosegruppen
            #endregion

            RuleFor(x => x)
                .Custom((y, ctx) =>
                {
                    var list = y.Diagnoses;

                    var palliativeItemsCount = list.Count(x => x == DiagnosisGroup.UndefinedDiagnosisGroup);

                    if (palliativeItemsCount > 0)
                    {
                        ctx.AddFailure(new ValidationFailure(nameof(Person.Diagnoses),
                            Validationmessages.AtLeastOneDiagnosisGroup(y.GetDisplayName())));
                    }
                });
        }
    }
}