using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.Payments;

public class Payment
{
    public int Id {get; set;}
    public decimal Amount {get; set;} 
    public DateOnly PaymentDate {get; set;}
    public DateOnly BillingPeriod {get; set;}
    public int UserId {get; set;}
    public User User {get; set;} = null!;
}