using FluentValidation;

namespace ScholarshipPlatform.ScholarshipApplications.Dtos;

public class CreateApplicationDtoValidator : AbstractValidator<CreateApplicationDto>
{
    public CreateApplicationDtoValidator()
    {
        RuleFor(x => x.SelectedCourseIds)
            .NotEmpty().WithMessage("Selecione pelo menos um curso.")
            .Must(list => list.Count <= 3).WithMessage("Você pode selecionar no máximo 3 cursos.")
            .Must(list => list.Distinct().Count() == list.Count).WithMessage("Não é permitido selecionar cursos duplicados.");
    }
}