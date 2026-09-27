using System.Security.Claims;
using ScholarshipPlatform.Notifications.Dtos;

namespace ScholarshipPlatform.Notifications;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notifications");

        // Disparo de notificações direcionadas (Apenas Admin e Mentor)
        group.MapPost("/dispatch", DispatchNotification)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Mentor"));

        // O usuário logado consulta a sua própria caixa de notificações
        group.MapGet("/me", GetMyNotifications)
            .RequireAuthorization();

        // Marcar uma notificação como lida
        group.MapPatch("/{id:int}/read", MarkAsRead)
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> DispatchNotification(
        SendNotificationRequestDto dto,
        NotificationService notificationService)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
            return TypedResults.BadRequest(new { message = "Título e mensagem são obrigatórios." });

        var result = await notificationService.DispatchNotificationAsync(dto);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetMyNotifications(
        ClaimsPrincipal user,
        bool? unreadOnly,
        NotificationService notificationService)
    {
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return TypedResults.Unauthorized();

        var notifications = await notificationService.GetMyNotificationsAsync(userId, unreadOnly ?? false);
        return TypedResults.Ok(notifications);
    }

    private static async Task<IResult> MarkAsRead(
        int id,
        ClaimsPrincipal user,
        NotificationService notificationService)
    {
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return TypedResults.Unauthorized();

        var success = await notificationService.MarkAsReadAsync(id, userId);
        return success ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}