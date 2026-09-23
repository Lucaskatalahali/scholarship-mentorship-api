namespace ScholarshipPlatform.Notifications;

public class NewsletterSubscription
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }

    // Token único seguro para gerar o link de opt-out (descadastro com 1 clique)
    public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");
}