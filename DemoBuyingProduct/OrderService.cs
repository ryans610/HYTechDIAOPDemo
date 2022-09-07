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
        if (await IsUserValidAsync(userId))
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }

        // reserve product for the session
        var sessionId = await ReserveProductsAsync(productId, quantity);
        if (sessionId == Guid.Empty)
        {
            // api return empty guid when product is not enough
            LogProductNotEnough(productId, quantity);

            return false;
        }

        // calculate total price
        var totalPrice = await CalculateTotalPriceAsync(productId, quantity);

        // save order
        var orderId = await SaveOrderAsync(userId, productId, quantity, totalPrice);

        // complete product reserve session
        await CompleteProductReserveSessionAsync(sessionId);

        // notify user for order established
        await NotifyOrderEstablishedAsync(orderId);

        return true;
    }

    private static async Task NotifyOrderEstablishedAsync(Guid orderId)
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

    private static async Task CompleteProductReserveSessionAsync(Guid sessionId)
    {
        await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/complete",
            new { sessionId });
    }

    private static async Task<Guid> SaveOrderAsync(Guid userId, Guid productId, int quantity, int totalPrice)
    {
        await using var connection = new SqlConnection("my connection string");
        await connection.OpenAsync();
        var orderId = await connection.ExecuteScalarAsync<Guid>(
            "spSaveOrder",
            new { userId, productId, quantity, totalPrice },
            commandType: CommandType.StoredProcedure);
        return orderId;
    }

    private static async Task<int> CalculateTotalPriceAsync(Guid productId, int quantity)
    {
        var productPriceResponse = await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/getPrice",
            new { productId });
        productPriceResponse.EnsureSuccessStatusCode();
        int price = await productPriceResponse.Content.ReadAsAsync<int>();
        int totalPrice = price * quantity;
        return totalPrice;
    }

    private void LogProductNotEnough(Guid productId, int quantity)
    {
        ILogger logger = new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger(this.GetType());
        logger.LogInformation(
            "商品{productId}數量少於{quantity}",
            productId, quantity);
    }

    private static async Task<Guid> ReserveProductsAsync(Guid productId, int quantity)
    {
        var productReserveResponse = await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/reserve",
            new { productId, quantity });
        productReserveResponse.EnsureSuccessStatusCode();
        var sessionId = await productReserveResponse.Content.ReadAsAsync<Guid>();
        return sessionId;
    }

    private static async Task<bool> IsUserValidAsync(Guid userId)
    {
        var userHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://my-users"),
        };
        var userResponse = await userHttpClient.PostAsJsonAsync(
            "api/isUserValid",
            new { userId });
        userResponse.EnsureSuccessStatusCode();
        var isUserValid = !await userResponse.Content.ReadAsAsync<bool>();
        return isUserValid;
    }
}

public class UserInvalidException : Exception
{
    public Guid UserId { get; set; }
}
