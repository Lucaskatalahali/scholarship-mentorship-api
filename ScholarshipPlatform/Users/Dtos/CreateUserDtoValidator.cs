using FluentValidation;
namespace ScholarshipPlatform.Users.Dtos;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(u => u.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(60).WithMessage("Name length cannot exceed 60 characters");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email is required."); 

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.");
        
        RuleFor(u => u.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .When(u => u.BirthDate is not null);

        RuleFor(u => u.Gpa)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(20)
            .When(u => u.Gpa is not null);
    }
}