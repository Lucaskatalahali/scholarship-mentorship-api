
using FluentValidation;

namespace ScholarshipPlatform.SuccessStories.Dtos;

public class PatchSuccessStoryDtoValidator : AbstractValidator<PatchSuccessStoryDto>
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
    public PatchSuccessStoryDtoValidator()
    {
        // 1. Nome do mentorado
        When(s => s.MenteeName is not null, () =>
        {
            RuleFor(s => s.MenteeName)
                .NotEmpty()
                .WithMessage("O nome do mentorado não pode ser vazio.")
                .MaximumLength(200)
                .WithMessage("O nome do mentorado deve ter no máximo 200 caracteres.");
        });

        // 2. Nome da bolsa
        When(s => s.ScholarshipName is not null, () =>
        {
            RuleFor(s => s.ScholarshipName)
                .NotEmpty()
                .WithMessage("O nome da bolsa não pode ser vazio.")
                .MaximumLength(200)
                .WithMessage("O nome da bolsa deve ter no máximo 200 caracteres.");
        });

        // 3. Nível académico
        When(s => s.AcademicLevel is not null, () =>
        {
            RuleFor(s => s.AcademicLevel)
                .IsInEnum()
                .WithMessage("O nível académico informado é inválido.");
        });

        // 4. País
        When(s => s.Country is not null, () =>
        {
            RuleFor(s => s.Country)
                .NotEmpty()
                .WithMessage("O país não pode ser vazio.")
                .MaximumLength(100)
                .WithMessage("O país deve ter no máximo 100 caracteres.");
        });

        // 5. Ano de aprovação
        When(s => s.ApprovalYear is not null, () =>
        {
            RuleFor(s => s.ApprovalYear)
                .InclusiveBetween(2000, DateTime.Now.Year)
                .WithMessage($"O ano de aprovação deve estar entre 2000 e {DateTime.Now.Year}.");
        });

        // 6. Instituição
        When(s => s.Institution is not null, () =>
        {
            RuleFor(s => s.Institution)
                .NotEmpty()
                .WithMessage("O nome da instituição não pode ser vazio.")
                .MaximumLength(200)
                .WithMessage("A instituição deve ter no máximo 200 caracteres.");
        });

        // 7. Citação
        When(s => s.Quote is not null, () =>
        {
            RuleFor(s => s.Quote)
                .NotEmpty()
                .WithMessage("A citação não pode ser vazia.")
                .MaximumLength(500)
                .WithMessage("A citação deve ter no máximo 500 caracteres.");
        });

        // 8. Testemunho
        When(s => s.Testimonial is not null, () =>
        {
            RuleFor(s => s.Testimonial)
                .NotEmpty()
                .WithMessage("O testemunho não pode ser vazio.");
        });

        // 9. URL do vídeo
        When(s => s.VideoUrl is not null, () =>
        {
            RuleFor(s => s.VideoUrl)
                .NotEmpty()
                .WithMessage("O link do vídeo não pode ser vazio.")
                .Must(url =>
                    Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                    (result.Scheme == Uri.UriSchemeHttp ||
                     result.Scheme == Uri.UriSchemeHttps))
                .WithMessage("O link do vídeo deve ser uma URL HTTP ou HTTPS válida.");
        });

        // 10. Destaque
        When(s => s.IsFeatured is not null, () =>
        {
            RuleFor(s => s.IsFeatured)
                .Must(value => value is true or false)
                .WithMessage("O valor de destaque é inválido.");
        });

        // 11. Imagem
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
