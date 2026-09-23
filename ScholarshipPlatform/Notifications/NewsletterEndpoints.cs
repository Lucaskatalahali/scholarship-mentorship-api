using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Notifications.Dtos;

namespace ScholarshipPlatform.Notifications;

public static class NewsletterEndpoints
{
    public static RouteGroupBuilder MapNewsletterEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/newsletter");

        // Visitante cadastra o e-mail na landing page
        group.MapPost("/subscribe", Subscribe)
            .AllowAnonymous();

        // Link de descadastro (opt-out) enviado no rodapé do e-mail
        group.MapGet("/unsubscribe/{token}", Unsubscribe)
            .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Subscribe(
        [FromBody] SubscribeNewsletterRequestDto dto,
        AppDbContext db)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var existing = await db.NewsletterSubscriptions
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail);

        if (existing is not null)
        {
            if (existing.IsActive)
            {
                return TypedResults.Ok(new NewsletterStatusResponseDto(
                    "Este e-mail já está inscrito para receber alertas de bolsas.", true));
            }

            // Se já existia mas tinha cancelado, reativa a inscrição
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            existing.UnsubscribeToken = Guid.NewGuid().ToString("N");
            await db.SaveChangesAsync();

            return TypedResults.Ok(new NewsletterStatusResponseDto(
                "Inscrição reativada com sucesso! Você voltará a receber novos alertas.", true));
        }

        var newSubscription = new NewsletterSubscription
        {
            Email = normalizedEmail
        };

        db.NewsletterSubscriptions.Add(newSubscription);
        await db.SaveChangesAsync();

        return TypedResults.Created(
            $"/newsletter/status",
            new NewsletterStatusResponseDto("Inscrição realizada com sucesso! Você receberá alertas de novas bolsas.", true));
    }

    private static async Task<IResult> Unsubscribe(
        string token,
        AppDbContext db)
    {
        var subscription = await db.NewsletterSubscriptions
            .FirstOrDefaultAsync(s => s.UnsubscribeToken == token);

        if (subscription is null)
        {
            return TypedResults.NotFound(new NewsletterStatusResponseDto("Token de cancelamento inválido ou expirado.", false));
        }

        if (!subscription.IsActive)
        {
            return TypedResults.Ok(new NewsletterStatusResponseDto("Esta inscrição já foi cancelada anteriormente.", false));
        }

        subscription.IsActive = false;
        subscription.UnsubscribedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return TypedResults.Ok(new NewsletterStatusResponseDto(
            "Você foi descadastrado com sucesso e não receberá mais comunicados desta lista.", false));
    }
}