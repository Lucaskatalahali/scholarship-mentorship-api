using FluentValidation;

namespace ScholarshipPlatform.Payments.Dtos;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentDtoValidator()
    {
        RuleFor(u => u.UserEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email is required."); 

        RuleFor(p => p.Amount)
            .GreaterThan(0).WithMessage("Invalid payment amount.");
    }
}