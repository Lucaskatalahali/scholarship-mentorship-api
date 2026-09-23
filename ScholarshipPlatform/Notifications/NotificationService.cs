using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Email;
using ScholarshipPlatform.Notifications.Dtos;
using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.Notifications;

public class NotificationService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public NotificationService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<SendNotificationResultDto> DispatchNotificationAsync(SendNotificationRequestDto dto)
    {
        var filter = dto.Filter;

        // 1. Inicia a consulta base com usuários ativos
        var query = _db.Users.AsNoTracking().Where(u => u.AccountStatus == AccountStatus.Active);

        // 2. Garante que critérios acadêmicos e financeiros afetem apenas quem é Mentorando
        bool requiresMentorandoRole = 
            filter.OnlyUnpaidLastMonth == true || 
            filter.MinGpa.HasValue || 
            filter.MinAge.HasValue || 
            filter.MaxAge.HasValue || 
            filter.ScholarshipId.HasValue;

        if (requiresMentorandoRole)
        {
            query = query.Where(u => _db.UserRoles.Any(ur =>
                ur.UserId == u.Id &&
                _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Mentorando")));
        }

        // 3. Aplica os filtros específicos de usuários
        if (filter.SpecificUserId.HasValue)
        {
            query = query.Where(u => u.Id == filter.SpecificUserId.Value);
        }
        else if (!filter.AllUsers)
        {
            // Candidatos de uma bolsa específica
            if (filter.ScholarshipId.HasValue)
            {
                query = query.Where(u => _db.ScholarshipApplications
                    .Any(app => app.UserId == u.Id && app.ScholarshipId == filter.ScholarshipId.Value));
            }

            // Faixa etária
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (filter.MinAge.HasValue)
            {
                var maxBirthDate = today.AddYears(-filter.MinAge.Value);
                query = query.Where(u => u.BirthDate <= maxBirthDate);
            }

            if (filter.MaxAge.HasValue)
            {
                var minBirthDate = today.AddYears(-filter.MaxAge.Value - 1).AddDays(1);
                query = query.Where(u => u.BirthDate >= minBirthDate);
            }

            // Nota / Média
            if (filter.MinGpa.HasValue)
            {
                query = query.Where(u => u.Average >= filter.MinGpa.Value);
            }

            // Mentorandos sem pagamento registrado para o mês anterior
            if (filter.OnlyUnpaidLastMonth == true)
            {
                var nowUtc = DateTime.UtcNow;
                var previousMonth = nowUtc.AddMonths(-1);
                var previousPeriod = new DateOnly(previousMonth.Year, previousMonth.Month, 1);

                query = query.Where(u => !_db.Payments.Any(p =>
                    p.UserId == u.Id &&
                    p.BillingPeriod == previousPeriod));
            }
        }

        // 4. Busca os usuários internos alvo
        var targetUsers = await query
            .Select(u => new { u.Id, u.Email, u.Name })
            .ToListAsync();

        // 5. Criação em lote das notificações internas (sininho) se houver usuários internos
        if (targetUsers.Count > 0)
        {
            var notifications = targetUsers.Select(u => new Notification
            {
                UserId = u.Id,
                Title = dto.Title,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            }).ToList();

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }

        int subscribersNotifiedCount = 0;

        // 6. Envio de e-mails
        if (dto.SendEmail)
        {
            // E-mails para usuários internos
            foreach (var user in targetUsers)
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    await _emailService.SendEmailAsync(user.Email, dto.Title, dto.Message);
                }
            }

            // E-mails para assinantes da newsletter (se solicitado)
            if (filter.IncludeSubscribers)
            {
                var subscribers = await _db.NewsletterSubscriptions
                    .AsNoTracking()
                    .Where(s => s.IsActive)
                    .Select(s => new { s.Email, s.UnsubscribeToken })
                    .ToListAsync();

                subscribersNotifiedCount = subscribers.Count;

                foreach (var sub in subscribers)
                {
                    var unsubscribeFooter = $"\n\nPara cancelar o recebimento de alertas, acesse o link:\nhttps://localhost:5274/newsletter/unsubscribe/{sub.UnsubscribeToken}";
                    var fullMessage = dto.Message + unsubscribeFooter;

                    await _emailService.SendEmailAsync(sub.Email, dto.Title, fullMessage);
                }
            }
        }

        int totalDelivered = targetUsers.Count + subscribersNotifiedCount;

        if (totalDelivered == 0)
        {
            return new SendNotificationResultDto(0, "Nenhum usuário ou assinante corresponde aos critérios selecionados.");
        }

        return new SendNotificationResultDto(
            totalDelivered,
            $"Notificação processada com sucesso: {targetUsers.Count} usuário(s) interno(s) e {subscribersNotifiedCount} assinante(s) de newsletter."
        );
    }

    public async Task<List<NotificationResponseDto>> GetMyNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponseDto(
                n.Id,
                n.Title,
                n.Message,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt
            ))
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification is null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }
}