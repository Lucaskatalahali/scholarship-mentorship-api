using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.Payments;

public class PaymentService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;

    public PaymentService(UserManager<User> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<ServiceResult<PaymentResponseDto>> CreatePayment(CreatePaymentDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.UserEmail);

        if(user is null)
        {
            return ServiceResult<PaymentResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["UserEmail"] = ["User not found."]
                });   
        }

        //Verificar se o usuário está tentando pagar o mesmo periodo já pago
        var paymentExists = await _db.Payments
            .AnyAsync(p => p.UserId == user.Id && p.BillingPeriod == dto.BillingPeriod);

        if (paymentExists)
        {
            return ServiceResult<PaymentResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["Payment"] = ["User has already a payment for this billing period."]
                });
        }


        //Criar o pagamento
        var payment = new Payment
        {
            Amount = dto.Amount,
            BillingPeriod = dto.BillingPeriod,
            PaymentDate = DateOnly.FromDateTime(DateTime.Today),
            UserId = user.Id
        };

        _db.Payments.Add(payment);

        await _db.SaveChangesAsync();

        //Verificar se a conta estava suspensa para reactivar

        if(user.AccountStatus == AccountStatus.SuspendedByDebt)
            user.AccountStatus = AccountStatus.Active;

        var paymentResponseDto = new PaymentResponseDto(
            payment.Id,
            payment.Amount,
            payment.PaymentDate,
            payment.BillingPeriod,
            user.Id,
            user.Name,
            user.Email!
        );

        return ServiceResult<PaymentResponseDto>.Success(paymentResponseDto);
    }

    public async Task<PaymentResponseDto?> GetPaymentById(int id)
    {
        var paymentResponseDto = await _db.Payments
        .Where(p => p.Id == id)
        .Select(p => new PaymentResponseDto(
            p.Id,
            p.Amount,
            p.PaymentDate,
            p.BillingPeriod,
            p.User.Id,
            p.User.Name,
            p.User.Email!
        )).FirstOrDefaultAsync();

        return paymentResponseDto;
    }

    public async Task<List<PaymentResponseDto>> GetPayments()
    {
        return await _db.Payments
            .Select(p => new PaymentResponseDto(
                p.Id,
                p.Amount,
                p.PaymentDate,
                p.BillingPeriod,
                p.User.Id,
                p.User.Name,
                p.User.Email!
            )).ToListAsync();
    }
    
    public async Task<List<PaymentResponseDto>> GetPaymentsByUser(string userEmail)
    {
        return await _db.Payments
            .Where(p => p.User.Email == userEmail)
            .Select(p => new PaymentResponseDto(
                p.Id,
                p.Amount,
                p.PaymentDate,
                p.BillingPeriod,
                p.User.Id,
                p.User.Name,
                p.User.Email!
            )).ToListAsync();
    }
}