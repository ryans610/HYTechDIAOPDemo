using MailKit.Net.Smtp;
using MimeKit;

namespace DemoBuyingProduct;

public class EmailAdapter : INotify
{
    public async Task NotifyManagerAsync(string subject, string text)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("from email"));
        message.To.Add(MailboxAddress.Parse("manager's email"));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = text,
        };
        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync("my smtp host");
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
