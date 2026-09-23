using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Feedbacks.Dtos;

namespace ScholarshipPlatform.Feedbacks;

public class FeedbackService
{
    private readonly AppDbContext _db;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(AppDbContext db, ILogger<FeedbackService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<FeedbackResponseDto>> SubmitFeedback(CreateFeedbackDto dto, int? userId = null)
    {
        var feedback = new Feedback
        {
            Type = dto.Type,
            Status = FeedbackStatus.New,
            Message = dto.Message.Trim(),
            UserId = userId,
            ContactEmail = dto.ContactEmail?.Trim(),
            ContactName = dto.ContactName?.Trim(),
            PageUrl = dto.PageUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Feedbacks.Add(feedback);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Novo feedback recebido: ID {FeedbackId}, Tipo {FeedbackType}, Usuário {UserId}",
            feedback.Id, feedback.Type, userId.HasValue ? userId.Value.ToString() : "Anônimo");

        var responseDto = new FeedbackResponseDto(
            feedback.Id,
            feedback.Type,
            feedback.Status,
            feedback.Message,
            feedback.UserId,
            feedback.ContactEmail,
            feedback.ContactName,
            feedback.PageUrl,
            feedback.CreatedAt
        );

        return ServiceResult<FeedbackResponseDto>.Success(responseDto);
    }

    public async Task<List<FeedbackResponseDto>> GetFeedbacks(FeedbackStatus? status = null, FeedbackType? type = null)
    {
        var query = _db.Feedbacks.AsNoTracking();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        if (type.HasValue)
            query = query.Where(f => f.Type == type.Value);

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackResponseDto(
                f.Id,
                f.Type,
                f.Status,
                f.Message,
                f.UserId,
                f.ContactEmail,
                f.ContactName,
                f.PageUrl,
                f.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<FeedbackResponseDto?> GetFeedbackById(int id)
    {
        return await _db.Feedbacks
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FeedbackResponseDto(
                f.Id,
                f.Type,
                f.Status,
                f.Message,
                f.UserId,
                f.ContactEmail,
                f.ContactName,
                f.PageUrl,
                f.CreatedAt
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateFeedbackStatus(int id, UpdateFeedbackStatusDto dto)
    {
        var feedback = await _db.Feedbacks.FindAsync(id);

        if (feedback is null) return false;

        feedback.Status = dto.Status;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Status do feedback {FeedbackId} atualizado para {NewStatus}", id, dto.Status);

        return true;
    }
}