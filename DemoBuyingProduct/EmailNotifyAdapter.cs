using MailKit.Net.Smtp;
using MimeKit;

namespace DemoBuyingProduct;

public class EmailNotifyAdapter : INotify
{
    public async Task NotifyOrderEstablishedAsync(Guid orderId)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("from email"));
        message.To.Add(MailboxAddress.Parse("user's email"));
        message.Subject = "訂單成立";
        message.Body = new TextPart("html")
        {
            Text = $"訂單已成立！<br>訂單編號：{orderId}",
        };
        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync("my smtp host");
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}