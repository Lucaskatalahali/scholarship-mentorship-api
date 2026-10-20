
using FluentValidation;

namespace ScholarshipPlatform.Scholarships.Dtos;

public class PatchScholarshipDtoValidator : AbstractValidator<PatchScholarshipDto>
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public PatchScholarshipDtoValidator()
    {
        // 1. Campos de texto
        When(s => s.Name is not null, () =>
        {
            RuleFor(s => s.Name)
                .NotEmpty()
                .WithMessage("O nome da bolsa não pode ser vazio.")
                .MaximumLength(200)
                .WithMessage("O nome da bolsa deve ter no máximo 200 caracteres.");
        });

        When(s => s.Title is not null, () =>
        {
            RuleFor(s => s.Title)
                .NotEmpty()
                .WithMessage("O título da bolsa não pode ser vazio.")
                .MaximumLength(200)
                .WithMessage("O título da bolsa deve ter no máximo 200 caracteres.");
        });

        When(s => s.Country is not null, () =>
        {
            RuleFor(s => s.Country)
                .NotEmpty()
                .WithMessage("O país de destino não pode ser vazio.");
        });

        When(s => s.Description is not null, () =>
        {
            RuleFor(s => s.Description)
                .NotEmpty()
                .WithMessage("A descrição da bolsa não pode ser vazia.");
        });

        // 2. Prazo de inscrição
        When(s => s.Deadline is not null, () =>
        {
            RuleFor(s => s.Deadline)
                .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("O prazo de inscrição não pode estar no passado.");
        });

        // 3. URL oficial
        When(s => s.OfficialUrl is not null, () =>
        {
            RuleFor(s => s.OfficialUrl)
                .NotEmpty()
                .WithMessage("O link oficial não pode ser vazio.")
                .Must(uri =>
                    Uri.TryCreate(uri, UriKind.Absolute, out var result) &&
                    (result.Scheme == Uri.UriSchemeHttp ||
                     result.Scheme == Uri.UriSchemeHttps))
                .WithMessage("O link oficial informado não é uma URL válida.");
        });

        // 4. Tipo de bolsa
        When(s => s.ScholarshipType is not null, () =>
        {
            RuleFor(s => s.ScholarshipType)
                .IsInEnum()
                .WithMessage("Tipo de bolsa inválido.");
        });

        // 5. Níveis de ensino
        When(s => s.Levels is not null, () =>
        {
            RuleFor(x => x.Levels)
                .Must(levels =>
                    !string.IsNullOrWhiteSpace(levels) &&
                    levels.Split(',', StringSplitOptions.TrimEntries)
                        .All(level =>
                            Enum.TryParse<ScholarshipLevel>(
                                level, true, out var parsed)
                            && Enum.IsDefined(parsed)))
                .WithMessage("Os níveis acadêmicos são inválidos ou estão vazios.");
        });

        // 6. Benefícios
        When(s => s.Benefits is not null, () =>
        {
            RuleFor(s => s.Benefits)
                .NotEmpty()
                .WithMessage("Se fornecida, a lista de benefícios não pode ser vazia.");

            RuleForEach(s => s.Benefits!)
                .NotEmpty()
                .WithMessage("Um benefício não pode ser vazio.");
        });

        // 7. Critérios de elegibilidade
        When(s => s.Eligibility is not null, () =>
        {
            RuleFor(s => s.Eligibility)
                .NotEmpty()
                .WithMessage("Se fornecida, a lista de elegibilidade não pode ser vazia.");

            RuleForEach(s => s.Eligibility!)
                .NotEmpty()
                .WithMessage("Um critério de elegibilidade não pode ser vazio.");
        });

        // 8. Documentos necessários
        When(s => s.RequiredDocuments is not null, () =>
        {
            RuleFor(s => s.RequiredDocuments)
                .NotEmpty()
                .WithMessage("Se fornecida, a lista de documentos não pode ser vazia.");

            RuleForEach(s => s.RequiredDocuments!)
                .NotEmpty()
                .WithMessage("O nome de um documento não pode ser vazio.");
        });

        // 9. Cursos associados
        When(s => s.CourseIds is not null, () =>
        {
            RuleFor(s => s.CourseIds)
                .NotEmpty()
                .WithMessage("Se enviada, a lista de cursos não pode estar vazia.")
                .Must(ids => ids is not null && ids.Distinct().Count() == ids.Count)
                .WithMessage("A lista de cursos não pode conter IDs duplicados.");

            RuleForEach(s => s.CourseIds!)
                .GreaterThan(0)
                .WithMessage("O ID do curso deve ser um número positivo.");
        });

        // 10. Imagem
        When(s => s.Image is not null, () =>
        {
            RuleFor(s => s.Image!)
                .Cascade(CascadeMode.Stop)
                .Must(file => file.Length > 0)
                .WithMessage("O arquivo de imagem enviado está vazio.")
                .Must(file => file.Length <= MaxImageSizeBytes)
                .WithMessage("A imagem deve ter no máximo 5 MB.")
                .Must(file => AllowedImageExtensions.Contains(
                    Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage(
                    "Formato de imagem inválido. Formatos aceitos: .jpg, .jpeg, .png, .webp.");
        });
    }
}
