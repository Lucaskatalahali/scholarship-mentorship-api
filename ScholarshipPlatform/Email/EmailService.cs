using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace ScholarshipPlatform.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task SendEmailAsync(string to, string subject, string body)
    {

        //Criando a mensagem
        var message = new MimeMessage();

        var senderName = _configuration["Email:SenderName"] ?? "Scholarship Platform";
        var senderEmail = _configuration["Email:From"]!;

        message.From.Add(
            new MailboxAddress(senderName, senderEmail));

        message.To.Add(
            MailboxAddress.Parse(to));

        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();

        //Conectar o backend ao servidor SMTP do Gmail
        await client.ConnectAsync(
            _configuration["Email:Host"]!,
            int.Parse(_configuration["Email:Port"]!),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _configuration["Email:Username"]!,
            _configuration["Email:Password"]!);

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}