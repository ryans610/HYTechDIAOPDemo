using System.Data;
using System.Data.SqlClient;
using System.Net.Http;

using Dapper;

using MailKit.Net.Smtp;

using MimeKit;

namespace DemoBuyingProduct;

public class OrderService
{
    public async Task<bool> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity)
    {
        // check is valid user
        var userHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://my-users"),
        };
        var userResponse = await userHttpClient.PostAsJsonAsync(
            "api/isUserValid",
            new { userId });
        userResponse.EnsureSuccessStatusCode();
        if (!await userResponse.Content.ReadAsAsync<bool>())
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }

        // check reserve product is enough
        await using var connection = new SqlConnection("my connection string");
        await connection.OpenAsync();
        int reserve = await connection.ExecuteScalarAsync<int>(
            "spGetProductReserveCount",
            new { productId },
            commandType: CommandType.StoredProcedure);
        if (reserve < quantity)
        {
            // notify manager
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("from email"));
            message.To.Add(MailboxAddress.Parse("manager's email"));
            message.Subject = "庫存不足";
            message.Body = new TextPart("html")
            {
                Text = $"商品{productId}庫存不足",
            };
            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync("my smtp host");
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);

            return false;
        }

        // save order
        var orderId = await connection.ExecuteScalarAsync<Guid>(
            "spSaveOrder",
            new { userId, productId, quantity },
            commandType: CommandType.StoredProcedure);

        // notify logistic for shipping product
        var logisticHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://my-logistic"),
        };
        var logisticResponse = await logisticHttpClient.PostAsJsonAsync(
            "api/shipping",
            new { orderId });
        logisticResponse.EnsureSuccessStatusCode();

        return true;
    }
}

public class UserInvalidException : Exception
{
    public Guid UserId { get; set; }
}
