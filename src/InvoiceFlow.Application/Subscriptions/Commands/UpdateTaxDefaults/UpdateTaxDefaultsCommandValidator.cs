using FluentValidation;

namespace InvoiceFlow.Application.Subscriptions.Commands.UpdateTaxDefaults;

public sealed class UpdateTaxDefaultsCommandValidator : AbstractValidator<UpdateTaxDefaultsCommand>
{
    public UpdateTaxDefaultsCommandValidator()
    {
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        RuleFor(x => x.TaxLabel).NotEmpty().MaximumLength(20);
    }
}
