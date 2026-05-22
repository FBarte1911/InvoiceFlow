using FluentValidation;

namespace InvoiceFlow.Application.Clients.Commands.CreateClient;

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(300);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone is not null);
        RuleFor(x => x.Company).MaximumLength(200).When(x => x.Company is not null);
    }
}
