using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Mkkp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Mkkp.Validation
{
    internal class DiagnosisGroupOnlyOnePalliativCareValidator : AbstractValidator<Person>
    {
        public DiagnosisGroupOnlyOnePalliativCareValidator()
        {
            #region Documentation
            // AreaDef: MKKP
            // OrderDef: 03
            // SectionDef: Klient
            // StrengthDef: Fehler

            // CheckDef: Erlaubte Werte
            // Fields: Diagnosegruppen, Remark: Nur eine Palliativ Diagnose Gruppe pro Klient, Group: Inhaltlich
            #endregion

            RuleFor(x => x)
                .Custom((y, ctx) =>
                {
                    var list = y.Diagnoses;

                    var palliativeItemsCount = list.Count(x => x == DiagnosisGroup.PalliativeCare1 ||
                                                               x == DiagnosisGroup.PalliativeCare2 ||
                                                               x == DiagnosisGroup.PalliativeCare3 ||
                                                               x == DiagnosisGroup.PalliativeCare4);

                    if (palliativeItemsCount > 1)
                    { 
                        ctx.AddFailure(new ValidationFailure(nameof(Person.Diagnoses),
                            Validationmessages.OnlyONePalliativeDiagnosisGroup (y.GetDisplayName())));
                    }
                });
        }
    }
}