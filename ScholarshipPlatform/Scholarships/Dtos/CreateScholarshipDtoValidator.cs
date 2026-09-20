using FluentValidation;
namespace ScholarshipPlatform.Scholarships.Dtos;

public class CreateScholarshipDtoValidator : AbstractValidator<CreateScholarshipDto>
{
    public CreateScholarshipDtoValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Scholarship name is required.");

        RuleFor(s => s.Country)
            .NotEmpty()
            .WithMessage("Country name is required.");

        RuleFor(s => s.Deadline)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Deadline cannot be in the past.");

        // 1. Garante que não é nulo/vazio e que os IDs são > 0
        RuleFor(x => x.CourseIds)
            .NotEmpty()
            .WithMessage("A bolsa deve estar associada a pelo menos um curso.")
            .ForEach(id => id.GreaterThan(0).WithMessage("O ID do curso deve ser um número positivo."));

        // 2. Regra dedicada para unicidade dos IDs
        RuleFor(x => x.CourseIds)
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
            .WithMessage("A lista de cursos não pode conter IDs duplicados.");
    }
}