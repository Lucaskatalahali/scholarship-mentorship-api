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

        RuleFor(s => s.Country)
            .NotEmpty()
            .When(s => s.Country is not null)
            .WithMessage("Country name is required.");

        RuleFor(s => s.Deadline)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .When(s => s.Deadline is not null)
            .WithMessage("Deadline cannot be in the past.");

        When(x => x.CourseIds is not null, () =>
        {
            // Regra 1: Valida cada ID individual dentro da lista
            RuleForEach(x => x.CourseIds)
                .GreaterThan(0)
                .WithMessage("O ID do curso deve ser um número positivo.");

            // Regra 2: Valida a coleção como um todo (evita duplicados e não pode ser vazia se enviada)
            RuleFor(x => x.CourseIds)
                .NotEmpty()
                .WithMessage("Se enviada, a lista de cursos não pode estar vazia.")
                .Must(ids => ids == null || new HashSet<int>(ids).Count == ids.Count)
                .WithMessage("A lista de cursos não pode conter IDs duplicados.");
        });
    }
}