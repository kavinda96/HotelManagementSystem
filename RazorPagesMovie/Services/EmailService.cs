using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IHotelInfoService _hotelInfoService;

    public EmailService(IHotelInfoService hotelInfoService)
    {
        _hotelInfoService = hotelInfoService;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Retrieve email settings
        var emailSettings = await _hotelInfoService.GetEmailSettingsAsync();

        // Extract settings or use defaults
        var smtpServer = emailSettings.TryGetValue("SmtpServer", out var server) ? server : "default.smtp.server";
        var smtpPort = emailSettings.TryGetValue("SmtpPort", out var port) ? int.Parse(port) : 25;
        var senderEmail = emailSettings.TryGetValue("SenderEmail", out var email) ? email : "default@hotel.com";
        var senderName = emailSettings.TryGetValue("SenderName", out var name) ? name : "Default Hotel";
        var senderPassword = emailSettings.TryGetValue("SenderPassword", out var password) ? password : "defaultpassword";

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
