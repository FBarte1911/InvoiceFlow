using FluentValidation;

namespace InvoiceFlow.Application.Invoicing.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date must be after or equal to issue date.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Invoice must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
