using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Payments.Dtos;
using ScholarshipPlatform.Users;
using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Payments;

public class PaymentService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        UserManager<User> userManager, 
        AppDbContext db,
        ILogger<PaymentService> logger)
    {
        _userManager = userManager;
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<PaymentResponseDto>> CreatePayment(CreatePaymentDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.UserEmail);

        if (user is null)
        {
            _logger.LogWarning("Tentativa de registrar pagamento para e-mail inexistente: {Email}", dto.UserEmail);
            return ServiceResult<PaymentResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["UserEmail"] = ["User not found."]
                });   
        }

        // Verificar se o usuário está tentando pagar o mesmo período já pago
        var paymentExists = await _db.Payments
            .AnyAsync(p => p.UserId == user.Id && p.BillingPeriod == dto.BillingPeriod);

        if (paymentExists)
        {
            _logger.LogWarning("Pagamento duplicado rejeitado para o usuário {UserId} referente ao período {BillingPeriod}", 
                user.Id, dto.BillingPeriod);

            return ServiceResult<PaymentResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["Payment"] = ["User has already a payment for this billing period."]
                });
        }

        // Criar o pagamento
        var payment = new Payment
        {
            Amount = dto.Amount,
            BillingPeriod = dto.BillingPeriod,
            PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = dto.Description,
            UserId = user.Id
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Pagamento {PaymentId} no valor de {Amount} registrado com sucesso para o usuário {UserId} referente a {BillingPeriod}",
            payment.Id, payment.Amount, user.Id, payment.BillingPeriod);

        // Verificar se a conta estava suspensa para reativar
        if (user.AccountStatus == AccountStatus.SuspendedByDebt)
        {
            user.AccountStatus = AccountStatus.Active;
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Conta do usuário {UserId} reativada após regularização de pagamento", user.Id);
        }

        var paymentResponseDto = new PaymentResponseDto(
            payment.Id,
            payment.Amount,
            payment.PaymentDate,
            payment.BillingPeriod,
            user.Id,
            user.Name,
            user.Email!,
            payment.Description
        );

        return ServiceResult<PaymentResponseDto>.Success(paymentResponseDto);
    }

    public async Task<PaymentResponseDto?> GetPaymentById(int id)
    {
        return await _db.Payments
            .Where(p => p.Id == id)
            .Select(p => new PaymentResponseDto(
                p.Id,
                p.Amount,
                p.PaymentDate,
                p.BillingPeriod,
                p.User.Id,
                p.User.Name,
                p.User.Email!,
                p.Description
            )).FirstOrDefaultAsync();
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
                p.User.Email!,
                p.Description
            )).ToListAsync();
    }

    public async Task<List<UserResponseDto>> GetUnpaidPayments()
    {
        var nowUtc = DateTime.UtcNow;
        var previousMonth = nowUtc.AddMonths(-1);

        var previousPeriod = new DateOnly(
            previousMonth.Year,
            previousMonth.Month,
            1
        );

        var mentorandoRoleId = await _db.Roles
            .Where(r => r.Name == "Mentorando")
            .Select(r => r.Id)
            .SingleAsync();

        var usersWithoutPayment = await _db.Users
            .Where(u => u.AccountStatus == AccountStatus.Active && _db.UserRoles.Any(r =>
                r.UserId == u.Id &&
                r.RoleId == mentorandoRoleId))
            .Where(u => !_db.Payments.Any(p =>
                p.UserId == u.Id &&
                p.BillingPeriod == previousPeriod))
            .Select(u => new UserResponseDto(
                u.Id,
                u.Name,
                u.Email!,
                u.BirthDate,
                u.Address,
                u.EducationLevel,
                u.Average,
                u.AccountStatus
            )).ToListAsync();
        
        return usersWithoutPayment;
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
                p.User.Email!,
                p.Description
            )).ToListAsync();
    }

    public async Task<int> ProcessPreviousPeriodPayments()
    {
        var nowUtc = DateTime.UtcNow;
        var previousMonth = nowUtc.AddMonths(-1);
        var previousPeriod = new DateOnly(previousMonth.Year, previousMonth.Month, 1);

        var mentorandoRoleId = await _db.Roles
            .Where(r => r.Name == "Mentorando")
            .Select(r => r.Id)
            .SingleAsync();

        var suspendedCount = await _db.Users
            .Where(u => u.AccountStatus == AccountStatus.Active && 
                _db.UserRoles.Any(r => r.UserId == u.Id && r.RoleId == mentorandoRoleId))
            .Where(u => !_db.Payments.Any(p => 
                p.UserId == u.Id && 
                p.BillingPeriod == previousPeriod))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.AccountStatus, AccountStatus.SuspendedByDebt));

        _logger.LogInformation("Rotina de cobrança executada para o período {BillingPeriod}. Total de mentorandos suspensos por dívida: {SuspendedCount}",
            previousPeriod, suspendedCount);

        return suspendedCount;
    }
}