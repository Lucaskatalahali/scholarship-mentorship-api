using FluentValidation;

namespace ScholarshipPlatform.SuccessStories.Dtos;

public class CreateSuccessStoryDtoValidator : AbstractValidator<CreateSuccessStoryDto>
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
    public CreateSuccessStoryDtoValidator()
    {
        RuleFor(s => s.MenteeName)
            .NotEmpty()
            .WithMessage("O nome do mentorado é obrigatório.")
            .MaximumLength(200)
            .WithMessage("O nome do mentorado deve ter no máximo 200 caracteres.");

        RuleFor(s => s.ScholarshipName)
            .NotEmpty()
            .WithMessage("O nome da bolsa é obrigatório.")
            .MaximumLength(200)
            .WithMessage("O nome da bolsa deve ter no máximo 200 caracteres.");

        RuleFor(s => s.AcademicLevel)
            .NotEmpty()
            .WithMessage("O nível acadêmico é obrigatório.")
            .IsInEnum()
            .WithMessage("Nível acadêmico inválido.");

        RuleFor(s => s.Country)
            .NotEmpty()
            .WithMessage("O país de destino é obrigatório.");

        RuleFor(s => s.ApprovalYear)
            .InclusiveBetween(2000, DateTime.Now.Year)
            .WithMessage($"O ano de aprovação deve estar entre 2000 e {DateTime.Now.Year}.");

        When(s => s.Institution is not null, () =>
        {
            RuleFor(s => s.Institution)
                .NotEmpty()
                .WithMessage("O nome da instituição é obrigatório.")
                .MaximumLength(150)
                .WithMessage("O nome da instituição deve ter no máximo 150 caracteres");
        });

        When(s => s.Quote is not null, () =>
        {
            RuleFor(s => s.Quote)
                .NotEmpty()
                .WithMessage("A citação é obrigatória.")
                .MaximumLength(500)
                .WithMessage("A citação deve ter no máximo 500 caracteres");
        });

        When(s => s.Testimonial is not null, () =>
        {
            RuleFor(s => s.Testimonial)
                .NotEmpty()
                .WithMessage("O depoimento é obrigatório.")
                .MaximumLength(2000)
                .WithMessage("O depoimento deve ter no máximo 2000 caracteres");
        });

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