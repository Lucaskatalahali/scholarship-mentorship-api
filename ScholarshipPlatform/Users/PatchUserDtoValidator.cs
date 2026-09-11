using FluentValidation;
namespace ScholarshipPlatform.Users;

public class PatchUserDtoValidator : AbstractValidator<PatchUserDto>
{
    public PatchUserDtoValidator()
    {
        When(u => u.Name is not null, () =>
        {
            RuleFor(u => u.Name)
                .NotEmpty()
                .WithMessage("Name cannot be empty.")
                .MaximumLength(60)
                .WithMessage("Name length cannot exceed 60 characters");
        });
        
        RuleFor(u => u.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .When(u => u.BirthDate is not null);

        RuleFor(u => u.Gpa)
            .GreaterThanOrEqualTo(0)
            .When(u => u.Gpa is not null)
            .LessThanOrEqualTo(20)
            .When(u => u.Gpa is not null);
    }
}