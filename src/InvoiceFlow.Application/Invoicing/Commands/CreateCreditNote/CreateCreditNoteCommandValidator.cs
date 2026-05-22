using FluentValidation;

namespace InvoiceFlow.Application.Invoicing.Commands.CreateCreditNote;

public sealed class CreateCreditNoteCommandValidator : AbstractValidator<CreateCreditNoteCommand>
{
    public CreateCreditNoteCommandValidator()
    {
        RuleFor(x => x.OriginalInvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
