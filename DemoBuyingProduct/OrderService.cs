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
        if (!await IsUserValidAsync(userId))
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }

        // check reserve product is enough
        int reserve = await GetProductReserveAsync(productId);
        if (reserve < quantity)
        {
            await NotifyManagerAsync(productId);

            return false;
        }

        // save order
        var orderId = await SaveOrderAsync(userId, productId, quantity);

        await NotifyLogisticForShippingProduct(orderId);

        return true;
    }

    private async Task NotifyLogisticForShippingProduct(Guid orderId)
    {
        // notify logistic for shipping product
        var logisticHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://my-logistic"),
        };
        var logisticResponse = await logisticHttpClient.PostAsJsonAsync(
            "api/shipping",
            new { orderId });
        logisticResponse.EnsureSuccessStatusCode();
    }

    private async Task<Guid> SaveOrderAsync(Guid userId, Guid productId, int quantity)
    {
        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<Guid>(
            "spSaveOrder",
            new { userId, productId, quantity },
            commandType: CommandType.StoredProcedure);
    }

    private async Task NotifyManagerAsync(Guid productId)
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
    }

    private async Task<int> GetProductReserveAsync(Guid productId)
    {
        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "spGetProductReserveCount",
            new { productId },
            commandType: CommandType.StoredProcedure);
    }

    private async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection("my connection string");
        await connection.OpenAsync();
        return connection;
    }

    private async Task<bool> IsUserValidAsync(Guid userId)
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
        var isUserValid = await userResponse.Content.ReadAsAsync<bool>();
        return isUserValid;
    }
}

public class UserInvalidException : Exception
{
    public Guid UserId { get; set; }
}
