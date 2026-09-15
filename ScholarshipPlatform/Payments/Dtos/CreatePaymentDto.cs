namespace ScholarshipPlatform.Payments.Dtos;

public record CreatePaymentDto(
    decimal Amount,
    DateOnly BillingPeriod,
    string UserEmail
);