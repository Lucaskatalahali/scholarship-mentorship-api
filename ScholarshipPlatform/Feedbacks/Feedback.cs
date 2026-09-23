namespace ScholarshipPlatform.Feedbacks;

public enum FeedbackType
{
    Suggestion, // Sugestão de melhoria
    BugReport, // Relato de erro/bug
    Praise, // Elogio
    Complaint, // Reclamação
    Other // Outro assunto
}

public enum FeedbackStatus
{
    New, // Recém-chegado (não lido)
    InReview, // Em análise pela equipe
    Resolved, // Resolvido/Respondido
    Dismissed // Descartado
}

public class Feedback
{
    public int Id { get; set; }
    
    public FeedbackType Type { get; set; }
    public FeedbackStatus Status { get; set; } = FeedbackStatus.New;
    
    public string Message { get; set; } = string.Empty;
    
    // Se o usuário estiver autenticado:
    public int? UserId { get; set; }
    
    // Se for um visitante anônimo que deixou contato voluntariamente:
    public string? ContactEmail { get; set; }
    public string? ContactName { get; set; }
    
    // Metadados úteis para reproduzir bugs:
    public string? PageUrl { get; set; }        // Em qual página do frontend ele estava
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}