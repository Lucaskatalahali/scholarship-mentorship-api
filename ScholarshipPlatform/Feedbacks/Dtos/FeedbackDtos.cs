using FluentValidation;

namespace ScholarshipPlatform.Feedbacks.Dtos;

public record CreateFeedbackDto(
    FeedbackType Type,
    string Message,
    string? ContactEmail,
    string? ContactName,
    string? PageUrl
);

public class CreateFeedbackDtoValidator : AbstractValidator<CreateFeedbackDto>
{
    public CreateFeedbackDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("O tipo de feedback informado é inválido.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("A mensagem do feedback não pode estar vazia.")
            .MinimumLength(5).WithMessage("A mensagem deve conter pelo menos 5 caracteres.")
            .MaximumLength(2000).WithMessage("A mensagem não pode exceder 2000 caracteres.");

        When(x => !string.IsNullOrWhiteSpace(x.ContactEmail), () =>
        {
            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("O formato do e-mail de contato informado é inválido.")
                .MaximumLength(150).WithMessage("O e-mail não pode exceder 150 caracteres.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ContactName), () =>
        {
            RuleFor(x => x.ContactName)
                .MaximumLength(100).WithMessage("O nome de contato não pode exceder 100 caracteres.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PageUrl), () =>
        {
            RuleFor(x => x.PageUrl)
                .MaximumLength(300).WithMessage("A URL da página não pode exceder 300 caracteres.");
        });
    }
}

public record UpdateFeedbackStatusDto(
    FeedbackStatus Status
);

public class UpdateFeedbackStatusDtoValidator : AbstractValidator<UpdateFeedbackStatusDto>
{
    public UpdateFeedbackStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("O status de feedback informado é inválido.");
    }
}

public record FeedbackResponseDto(
    int Id,
    FeedbackType Type,
    FeedbackStatus Status,
    string Message,
    int? UserId,
    string? ContactEmail,
    string? ContactName,
    string? PageUrl,
    DateTime CreatedAt
);