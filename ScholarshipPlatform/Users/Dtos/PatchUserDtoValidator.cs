using FluentValidation;
namespace ScholarshipPlatform.Users.Dtos;

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

        When(u => u.Adress.Country is not null, () =>
        {
            RuleFor(u => u.Adress.Country)
                .NotEmpty().WithMessage("Country cannot be empty");
        });

        When(u => u.Adress.Province is not null, () =>
        {
            RuleFor(u => u.Adress.Province)
                .NotEmpty().WithMessage("Province cannot be empty");
        });

        When(u => u.Adress.AddressLine is not null, () =>
        {
            RuleFor(u => u.Adress.AddressLine)
                .NotEmpty().WithMessage("Country cannot be empty");
        });
        
        RuleFor(u => u.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .When(u => u.BirthDate is not null);

        RuleFor(u => u.Average)
            .GreaterThanOrEqualTo(0)
            .When(u => u.Average is not null)
            .LessThanOrEqualTo(20)
            .When(u => u.Average is not null);
    }
}