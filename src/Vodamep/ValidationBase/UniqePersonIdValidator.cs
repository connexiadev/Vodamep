using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using Vodamep.ReportBase;

namespace Vodamep.ValidationBase
{

    internal class UniqePersonIdValidator : AbstractValidator<IReport>
    {
        public UniqePersonIdValidator()
        {
            this.RuleFor(x => x.Persons)
                .Custom((list, ctx) =>
                {
                    foreach (var id in list.Select(x => x.Id).OrderBy(x => x).GroupBy(x => x).Where(x => x.Count() > 1))
                    {
                        var item = list.Where(x => x.Id == id.Key).First();
                        var index = list.IndexOf(item);
                        ctx.AddFailure(new ValidationFailure($"{nameof(ctx.PropertyPath)}[{index}]", Validationmessages.IdIsNotUnique));
                    }
                });
        }
    }

}