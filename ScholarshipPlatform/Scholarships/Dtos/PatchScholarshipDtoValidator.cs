using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ScholarshipPlatform.Scholarships.Dtos;

public class PatchScholarshipDtoValidator : AbstractValidator<PatchScholarshipDto>
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

    public PatchScholarshipDtoValidator()
    {
        // 1. Textos e URLs
        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("O nome da bolsa não pode ser vazio.")
            .MaximumLength(200).WithMessage("O nome da bolsa deve ter no máximo 200 caracteres.")
            .When(s => s.Name is not null);

        RuleFor(s => s.Title)
            .NotEmpty().WithMessage("O título da bolsa não pode ser vazio.")
            .MaximumLength(200).WithMessage("O título da bolsa deve ter no máximo 200 caracteres.")
            .When(s => s.Title is not null);

        RuleFor(s => s.Country)
            .NotEmpty().WithMessage("O país de destino não pode ser vazio.")
            .When(s => s.Country is not null);

        RuleFor(s => s.Description)
            .NotEmpty().WithMessage("A descrição não pode ser vazia.")
            .When(s => s.Description is not null);

        RuleFor(s => s.Deadline)
            .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("O prazo de inscrição não pode estar no passado.")
            .When(s => s.Deadline is not null);

        RuleFor(s => s.OfficialUrl)
            .NotEmpty().WithMessage("O link oficial não pode ser vazio.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var result) 
                         && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            .WithMessage("O link oficial informado não é uma URL válida.")
            .When(s => s.OfficialUrl is not null);

        // 2. Enums
        RuleFor(s => s.ScholarshipType)
            .IsInEnum().WithMessage("Tipo de bolsa inválido.")
            .When(s => s.ScholarshipType is not null);

        // 3. Coleções
        When(s => s.Levels is not null, () =>
        {
            RuleFor(s => s.Levels)
                .NotEmpty().WithMessage("Se fornecida, a lista de níveis não pode ser vazia.");

            RuleForEach(s => s.Levels)
                .IsInEnum().WithMessage("Nível acadêmico inválido.");
        });

        When(s => s.Benefits is not null, () =>
        {
            RuleFor(s => s.Benefits)
                .NotEmpty().WithMessage("Se fornecida, a lista de benefícios não pode ser vazia.");

            RuleForEach(s => s.Benefits)
                .NotEmpty().WithMessage("Um benefício não pode ser vazio.");
        });

        When(s => s.Eligibility is not null, () =>
        {
            RuleFor(s => s.Eligibility)
                .NotEmpty().WithMessage("Se fornecida, a lista de elegibilidade não pode ser vazia.");

            RuleForEach(s => s.Eligibility)
                .NotEmpty().WithMessage("Um critério de elegibilidade não pode ser vazio.");
        });

        When(s => s.RequiredDocuments is not null, () =>
        {
            RuleFor(s => s.RequiredDocuments)
                .NotEmpty().WithMessage("Se fornecida, a lista de documentos necessários não pode ser vazia.");

            RuleForEach(s => s.RequiredDocuments)
                .NotEmpty().WithMessage("O nome de um documento não pode ser vazio.");
        });

        When(x => x.CourseIds is not null, () =>
        {
            RuleFor(x => x.CourseIds)
                .NotEmpty()
                .WithMessage("Se enviada, a lista de cursos não pode estar vazia.")
                .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
                .WithMessage("A lista de cursos não pode conter IDs duplicados.");

            RuleForEach(x => x.CourseIds)
                .GreaterThan(0)
                .WithMessage("O ID do curso deve ser um número positivo.");
        });

        // 4. Imagem física
        When(s => s.Image is not null, () =>
        {
            RuleFor(s => s.Image!.Length)
                .GreaterThan(0).WithMessage("O arquivo de imagem enviado está vazio.")
                .LessThanOrEqualTo(MaxImageSizeBytes).WithMessage("A imagem deve ter no máximo 5 MB.");

            RuleFor(s => s.Image!)
                .Must(file =>
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    return AllowedImageExtensions.Contains(ext);
                })
                .WithMessage("Formato de imagem inválido. Formatos aceitos: .jpg, .jpeg, .png, .webp.");
        });
    }
}