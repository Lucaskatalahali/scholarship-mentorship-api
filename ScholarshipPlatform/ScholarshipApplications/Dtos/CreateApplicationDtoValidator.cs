using FluentValidation;

namespace ScholarshipPlatform.ScholarshipApplications.Dtos;

public class CreateApplicationDtoValidator : AbstractValidator<ApplyScholarshipDto>
{
    public CreateApplicationDtoValidator()
    {
        RuleFor(x => x.ScholarshipId)
            .GreaterThan(0)
            .WithMessage("O identificador da bolsa deve ser válido.");

        RuleFor(x => x.SelectedLevel)
            .IsInEnum()
            .WithMessage("Nível acadêmico selecionado inválido.");

        RuleFor(x => x.SelectedCourseIds)
            .NotEmpty()
            .WithMessage("Selecione pelo menos um curso.")
            .Must(list => list.Count <= 3)
            .WithMessage("Você pode selecionar no máximo 3 cursos.")
            .Must(list => list.Distinct().Count() == list.Count)
            .WithMessage("Não é permitido selecionar cursos duplicados.");

        RuleForEach(x => x.SelectedCourseIds)
            .GreaterThan(0)
            .WithMessage("O identificador do curso deve ser um número positivo.");
    }
}