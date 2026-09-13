using FluentValidation;

namespace ScholarshipPlatform.Users;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email is required."); 

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}