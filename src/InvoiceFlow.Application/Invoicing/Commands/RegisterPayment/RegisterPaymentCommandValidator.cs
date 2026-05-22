using FluentValidation;

namespace InvoiceFlow.Application.Invoicing.Commands.RegisterPayment;

public sealed class RegisterPaymentCommandValidator : AbstractValidator<RegisterPaymentCommand>
{
    public RegisterPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaidAt).LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5));
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);
    }
}
