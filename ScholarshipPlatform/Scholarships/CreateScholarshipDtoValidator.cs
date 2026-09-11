using FluentValidation;
namespace ScholarshipPlatform.Scholarships;

public class CreateScholarshipDtoValidator : AbstractValidator<CreateScholarshipDto>
{
    public CreateScholarshipDtoValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Scholarship name is required.");

        RuleFor(s => s.Deadline)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Deadline cannot be in the past.");
    }
}