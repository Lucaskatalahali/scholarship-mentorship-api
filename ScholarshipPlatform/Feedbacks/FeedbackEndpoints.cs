using System.Security.Claims;
using ScholarshipPlatform.Feedbacks.Dtos;

namespace ScholarshipPlatform.Feedbacks;

public static class FeedbackEndpoints
{
    public static RouteGroupBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/feedbacks");

        // Qualquer usuário (logado ou visitante anônimo) pode enviar feedback
        group.MapPost("/", SubmitFeedback);

        // Apenas Admin pode listar feedbacks
        group.MapGet("/", GetFeedbacks)
             .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Apenas Admin pode ver detalhes de um feedback específico
        group.MapGet("/{id:int}", GetFeedbackById)
             .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Apenas Admin pode atualizar status (ex: New -> InReview -> Resolved)
        group.MapPatch("/{id:int}/status", UpdateFeedbackStatus)
             .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;
    }

    private static async Task<IResult> SubmitFeedback(
        CreateFeedbackDto dto,
        CreateFeedbackDtoValidator validator,
        FeedbackService feedbackService,
        ClaimsPrincipal user)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        // Se o usuário estiver autenticado, extrai o UserId do token JWT; senão, passa null
        int? userId = null;
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var result = await feedbackService.SubmitFeedback(dto, userId);

        return TypedResults.Created($"/feedbacks/{result.Data!.Id}", result.Data);
    }

    private static async Task<IResult> GetFeedbacks(
        FeedbackStatus? status,
        FeedbackType? type,
        FeedbackService feedbackService)
    {
        var feedbacks = await feedbackService.GetFeedbacks(status, type);
        return TypedResults.Ok(feedbacks);
    }

    private static async Task<IResult> GetFeedbackById(
        int id,
        FeedbackService feedbackService)
    {
        var feedback = await feedbackService.GetFeedbackById(id);

        return feedback is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(feedback);
    }

    private static async Task<IResult> UpdateFeedbackStatus(
        int id,
        UpdateFeedbackStatusDto dto,
        UpdateFeedbackStatusDtoValidator validator,
        FeedbackService feedbackService)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var succeeded = await feedbackService.UpdateFeedbackStatus(id, dto);

        return succeeded
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}