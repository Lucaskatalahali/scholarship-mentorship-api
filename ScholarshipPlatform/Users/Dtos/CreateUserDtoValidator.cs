using System.Data;
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
            .NotEmpty().WithMessage("Birth date is required.")
            .LessThan(DateOnly.FromDateTime(DateTime.Today));

        RuleFor(u => u.Gpa)
            .NotEmpty().WithMessage("GPA is required")
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(20);

        RuleFor(x => x.PhoneNumber)
        .NotEmpty().WithMessage("Phone number is required")
        .Matches(@"^\+?[1-9]\d{1,14}$")
        .WithMessage("O número de telefone deve conter formato internacional válido (ex: +244923000000).");

        RuleFor(u => u.Address.Country)
            .NotEmpty().WithMessage("Country is required,");

        RuleFor(u => u.Address.Province)
            .NotEmpty().WithMessage("Province is required.");

        RuleFor(u => u.Address.AddressLine)
            .NotEmpty().WithMessage("Address Line is required");
        
        RuleFor(u => u.EducationLevel)
            .NotEmpty().WithMessage("Educational Level is required.");
    }
}