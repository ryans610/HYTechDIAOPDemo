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

        // reserve product for the session
        var productHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        };
        var productReserveResponse = await productHttpClient.PostAsJsonAsync(
            "api/reserve",
            new { productId, quantity });
        productReserveResponse.EnsureSuccessStatusCode();
        var sessionId = await userResponse.Content.ReadAsAsync<Guid>();
        if (sessionId == Guid.Empty)
        {
            // api return empty guid when product is not enough
            ILogger logger = new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger(this.GetType());
            logger.LogInformation(
                "商品{productId}數量少於{quantity}",
                productId, quantity);

            return false;
        }

        // calculate total price
        var productPriceResponse = await productHttpClient.PostAsJsonAsync(
            "api/getPrice",
            new { productId });
        productPriceResponse.EnsureSuccessStatusCode();
        int price = await productPriceResponse.Content.ReadAsAsync<int>();
        int totalPrice = price * quantity;

        // save order
        await using var connection = new SqlConnection("my connection string");
        await connection.OpenAsync();
        var orderId = await connection.ExecuteScalarAsync<Guid>(
            "spSaveOrder",
            new { userId, productId, quantity, totalPrice },
            commandType: CommandType.StoredProcedure);

        // complete product reserve session
        await productHttpClient.PostAsJsonAsync(
            "api/complete",
            new { sessionId });

        // notify user for order established
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

        return true;
    }
}

public class UserInvalidException : Exception
{
    public Guid UserId { get; set; }
}
