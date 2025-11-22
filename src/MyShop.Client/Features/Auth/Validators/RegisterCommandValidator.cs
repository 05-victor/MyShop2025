using FluentValidation;
using MyShop.Client.Features.Auth.Commands;
using MyShop.Shared.Constants;

namespace MyShop.Client.Features.Auth.Validators;

/// <summary>
/// FluentValidation rules cho RegisterCommand
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ValidationMessages.UsernameRequired)
            .MinimumLength(3).WithMessage(ValidationMessages.UsernameTooShort)
            .MaximumLength(50).WithMessage(ValidationMessages.UsernameTooLong);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(6).WithMessage(ValidationMessages.PasswordTooShort)
            .MaximumLength(100).WithMessage(ValidationMessages.PasswordTooLong);
    }
}
