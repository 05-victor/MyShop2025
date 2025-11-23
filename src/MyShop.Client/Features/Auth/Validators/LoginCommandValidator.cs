using FluentValidation;
using MyShop.Client.Features.Auth.Commands;
using MyShop.Shared.Constants;

namespace MyShop.Client.Features.Auth.Validators;

/// <summary>
/// FluentValidation rules cho LoginCommand
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ValidationMessages.UsernameRequired)
            .MinimumLength(3).WithMessage(ValidationMessages.UsernameTooShort);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(6).WithMessage(ValidationMessages.PasswordTooShort);
    }
}
