namespace ScholarshipPlatform.Payments;

public record CreatePaymentDto(
    decimal Amount,
    DateOnly BillingPeriod,
    string UserEmail
);