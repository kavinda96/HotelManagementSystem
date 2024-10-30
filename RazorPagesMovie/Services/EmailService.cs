using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"];
        var senderPassword = _configuration["EmailSettings:SenderPassword"];

        var smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = smtpPort == 465 // Enable SSL only for port 465
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (SmtpException smtpEx)
        {
            // Log SMTP-specific errors
            Console.WriteLine($"SMTP Error: {smtpEx.Message}");
        }
        catch (Exception ex)
        {
            // Log general errors
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }
}
