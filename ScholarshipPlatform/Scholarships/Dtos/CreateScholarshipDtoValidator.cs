using FluentValidation;

namespace ScholarshipPlatform.Scholarships.Dtos;

public class CreateScholarshipDtoValidator : AbstractValidator<CreateScholarshipDto>
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public CreateScholarshipDtoValidator()
    {
        // 1. Campos de texto básicos
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("O nome da bolsa é obrigatório.")
            .MaximumLength(200)
            .WithMessage("O nome da bolsa deve ter no máximo 200 caracteres.");

        RuleFor(s => s.Title)
            .NotEmpty()
            .WithMessage("O título da bolsa (instituição/programa) é obrigatório.")
            .MaximumLength(200)
            .WithMessage("O título deve ter no máximo 200 caracteres.");
        
        RuleFor(s => s.Description)
            .NotEmpty()
            .WithMessage("A descrição da bolsa é obrigatório.");

        RuleFor(s => s.Country)
            .NotEmpty()
            .WithMessage("O país de destino é obrigatório.");

        RuleFor(s => s.Deadline)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("O prazo de inscrição (deadline) não pode estar no passado.");

        RuleFor(s => s.OfficialUrl)
            .NotEmpty()
            .WithMessage("O link oficial do edital/bolsa é obrigatório.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var result) 
                         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            .WithMessage("O link oficial informado não é uma URL válida.");

        // 2. Tipo de bolsa (Enum)
        RuleFor(s => s.ScholarshipType)
            .IsInEnum()
            .WithMessage("Tipo de bolsa inválido. Deve ser Total ou Parcial.");

        // 3. Níveis de ensino (Levels)
        RuleFor(x => x.Levels)
            .NotEmpty()
            .WithMessage("A bolsa deve ter pelo menos um nível de ensino.")
            .Must(levels =>
                levels.Split(',', StringSplitOptions.TrimEntries)
                    .All(level =>
                        Enum.TryParse<ScholarshipLevel>(
                            level, true, out var parsed)
                        && Enum.IsDefined(parsed)))
            .WithMessage("Um ou mais níveis acadêmicos são inválidos.");

        // 4. Listas de marcadores (Benefits, Eligibility, RequiredDocuments)
        RuleFor(s => s.Benefits)
            .NotEmpty()
            .WithMessage("Informe pelo menos um benefício da bolsa.")
            .ForEach(b => b.NotEmpty().WithMessage("O benefício não pode ser vazio."));

        RuleFor(s => s.Eligibility)
            .NotEmpty()
            .WithMessage("Informe pelo menos um critério de elegibilidade.")
            .ForEach(e => e.NotEmpty().WithMessage("O critério de elegibilidade não pode ser vazio."));

        RuleFor(s => s.RequiredDocuments)
            .NotEmpty()
            .WithMessage("Informe pelo menos um documento necessário.")
            .ForEach(d => d.NotEmpty().WithMessage("O nome do documento não pode ser vazio."));

        // 5. Cursos associados
        RuleFor(x => x.CourseIds)
            .NotEmpty()
            .WithMessage("A bolsa deve estar associada a pelo menos um curso.")
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
            .WithMessage("A lista de cursos não pode conter IDs duplicados.")
            .ForEach(id => id.GreaterThan(0).WithMessage("O ID do curso deve ser um número positivo."));
          

        // 6. Imagem física (IFormFile)
        RuleFor(s => s.Image)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("O arquivo de imagem é obrigatório.")
            .Must(file => file!.Length > 0)
            .WithMessage("O arquivo de imagem enviado está vazio.")
            .Must(file => file!.Length <= MaxImageSizeBytes)
            .WithMessage("A imagem deve ter no máximo 5 MB.")
            .Must(file => AllowedImageExtensions.Contains(
                Path.GetExtension(file!.FileName).ToLowerInvariant()))
            .WithMessage("Formato de imagem inválido. Formatos aceitos: .jpg, .jpeg, .png, .webp.");
    }
}