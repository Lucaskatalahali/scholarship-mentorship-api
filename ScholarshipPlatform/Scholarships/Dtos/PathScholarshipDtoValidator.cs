using FluentValidation;
namespace ScholarshipPlatform.Scholarships.Dtos;

public class PathScholarshipDtoValidator : AbstractValidator<PathScholarshipDto>
{
    public PathScholarshipDtoValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .When(s => s.Name is not null)
            .WithMessage("Scholarship name is required.");

        RuleFor(s => s.Deadline)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .When(s => s.Deadline is not null)
            .WithMessage("Deadline cannot be in the past.");
    }
}