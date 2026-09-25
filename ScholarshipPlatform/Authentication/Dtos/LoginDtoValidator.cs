using FluentValidation;

namespace ScholarshipPlatform.Authentication.Dtos;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .EmailAddress().WithMessage("A valid email is required."); 

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("É necessário informar um email válido.");
    }
}