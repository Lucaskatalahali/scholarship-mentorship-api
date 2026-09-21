namespace ScholarshipPlatform.Payments.Dtos;

public record PaymentResponseDto(
    int Id,
    decimal Amount,
    DateOnly PaymentDate,
    DateOnly BillingPeriod,
    int UserId,
    string UserName,
    string UserEmail, // User email acaba descrevendo melhor quem pagou
    string? Description
);