using ScholarshipPlatform.Payments.Dtos;

namespace ScholarshipPlatform.Payments;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/payments");

        group.MapPost("/", CreatePayment)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        group.MapGet("/{id:int}", GetPaymentById)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/", GetPayments)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

         group.MapGet("/unpaid", GetUnpaidPayments)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/user/{userEmail}", GetPaymentsByUser)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/previous-period", CheckPreviousPeriodPayments)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;
    }

    private static async Task<IResult> CreatePayment(
        CreatePaymentDto dto, 
        CreatePaymentDtoValidator validator, 
        PaymentService paymentService)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var result = await paymentService.CreatePayment(dto);

        if(!result.IsSuccess)
                return TypedResults.ValidationProblem(result.Errors!);

        return TypedResults.Created($"/payments/{result.Data!.Id}", result.Data);
    }

    private static async Task<IResult> GetPaymentById(int id, PaymentService paymentService)
    {
        if(id <= 0)
            return TypedResults.BadRequest("ID must be greater than 0");

        var paymentResponseDto = await paymentService.GetPaymentById(id);

        return paymentResponseDto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(paymentResponseDto);   
    }

    private static async Task<IResult> GetPayments(PaymentService paymentService)
    {
        var paymentsDto = await paymentService.GetPayments();

        return TypedResults.Ok(paymentsDto);
    }

    private static async Task<IResult> GetUnpaidPayments(PaymentService paymentService)
    {
        var paymentsDto = await paymentService.GetUnpaidPayments();

        return TypedResults.Ok(paymentsDto);
    }
    private static async Task<IResult> GetPaymentsByUser(string userEmail, PaymentService paymentService)
    {
        var paymentsDto = await paymentService.GetPaymentsByUser(userEmail);

        return TypedResults.Ok(paymentsDto);
    }

    private static async Task<IResult> CheckPreviousPeriodPayments(PaymentService paymentService)
    {
        await paymentService.CheckPreviousPeriodPayments();
        return TypedResults.NoContent();
    }
}