using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vodamep.Data;
using Vodamep.Hkpv.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Hkpv.Validation
{
    internal class StaffValidator : AbstractValidator<Staff>
    {
        public StaffValidator(DateTime from, DateTime to)
        {
            #region Documentation
            // AreaDef: HKP
            // OrderDef: 02
            // SectionDef: Mitarbeiter
            // StrengthDef: Fehler

            // CheckDef: Muss Feld
            // Fields: Nachname
            // Fields: Vorname
            // Fields: Qualifizierung, Remark: Ab 2019

            // CheckDef: Erlaubte Werte
            // Fields: Nachname, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            // Fields: Vorname, Remark: Buchstaben, Bindestrich, Leerzeichen, Punkt
            // Fields: Qualifizierung, Remark: Vor 2019 alle Bezeichnungen erlaubt, Strength: Warnung
            // Fields: Qualifizierung, Remark: Ab 2019 nur Qualifizierungs-Liste, Url: src/Vodamep/Datasets/QualificationCode.csv
            // Fields: Anstellung, Remark: Keine Überschneidungen der Anstellungszeiträume

            #endregion


            this.RuleFor(x => x.FamilyName).NotEmpty();
            this.RuleFor(x => x.GivenName).NotEmpty();

            this.RuleFor(x => x.Employments).Must(x => x != null && x.Count > 0).WithMessage(Validationmessages.StaffWithoutEmployment);

            // Änderung 5.11.2018, LH
            var r = new Regex(@"^[\p{L}][-\p{L} ]*[\p{L}]$");
            this.RuleFor(x => x.FamilyName).Matches(r).Unless(x => string.IsNullOrEmpty(x.FamilyName));

            // keine Einschränkungen für Vornamen: (z.B. Auszubildende 02) this.RuleFor(x => x.GivenName).Matches(r).Unless(x => string.IsNullOrEmpty(x.GivenName));

            // Änderung 27.11.2018, LH
            // Nicht für Bestandsdaten
            if (to < new DateTime(2019, 01, 01))
            {
                this.RuleFor(x => x.Qualification)
                .SetValidator(new CodeValidator<QualificationCodeProvider>())
                .Unless(x => string.IsNullOrEmpty(x.Qualification))
                .WithSeverity(Severity.Warning);
            }
            else
            {
                this.RuleFor(x => x.Qualification).NotEmpty();

                this.RuleFor(x => x.Qualification)
                .SetValidator(new CodeValidator<QualificationCodeProvider>())
                .Unless(x => string.IsNullOrEmpty(x.Qualification));
            }


            // Anstellung
            this.RuleForEach(x => x.Employments).SetValidator(x => new EmploymentValidator(from, to, x));

            // Anstellungszeiträume
            RuleFor(x => x)
                .Custom((staff, ctx) =>
                {
                    List<Employment> employments = staff.Employments?.OrderBy(x => x.FromD).ToList();
                    HkpvReport report = ctx.ParentContext.InstanceToValidate as HkpvReport;

                    for (int i = 1; i < employments.Count; i++)
                    {
                        Employment last = employments[i - 1];
                        Employment current = employments[i];

                        if (last.FromD != null && last.ToD != null &&
                            current.FromD != null && current.ToD != null)
                        {
                            if (last.FromD >= from && last.ToD <= to &&
                                current.FromD >= from && current.ToD <= to)
                            {
                                if (last.ToD >= current.FromD)
                                {
                                    var index = staff.Employments.IndexOf(current);
                                    ctx.AddFailure(new ValidationFailure(ctx.PropertyName, Validationmessages.EmploymentOverlap(staff)));

                                }
                            }
                        }
                    }
                });

        }
    }
}
