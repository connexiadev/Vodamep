﻿using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Vodamep.Mkkp.Model;
using Vodamep.ValidationBase;

namespace Vodamep.Mkkp.Validation
{
    internal class DiagnosisGroupIsUniqueValidator : AbstractValidator<Person>
    {
        public DiagnosisGroupIsUniqueValidator()
        {
            RuleFor(x => x)
                .Custom((y, ctx) =>
                {
                    var list = y.Diagnoses;

                    var duplicates = list
                        .GroupBy(x => x)
                        .Where(x => x.Count() > 1);

                    foreach (var entry in duplicates)
                    {
                        ctx.AddFailure(new ValidationFailure(nameof(Person.Diagnoses), Validationmessages.DoubledDiagnosisGroups(y.GetDisplayName())));
                    }
                });
        }

    }
}