using System.Data;
using System.Data.SqlClient;
using System.Net.Http;

using Dapper;

using MailKit.Net.Smtp;

using MimeKit;

namespace DemoBuyingProduct;

public class UserProxy
{
    public async Task<bool> IsUserValidAsync(Guid userId)
    {
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

public class ProductProxy
{
    public async Task CompleteProductReserveAsync(Guid sessionId)
    {
        await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/complete",
            new { sessionId });
    }

    public async Task<int> GetPriceAsync(Guid productId)
    {
        var productPriceResponse = await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/getPrice",
            new { productId });
        productPriceResponse.EnsureSuccessStatusCode();
        int price = await productPriceResponse.Content.ReadAsAsync<int>();
        return price;
    }

    public async Task<Guid> ReserveProductAsync(Guid productId, int quantity)
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
}

public class SmtpAdapter
{
    public async Task NotifyUserAsync(Guid orderId)
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

public class OrderDao
{
    public async Task<Guid> SaveOrderAsync(Guid userId, Guid productId, int quantity, int totalPrice)
    {
        await using var connection = new SqlConnection("my connection string");
        await connection.OpenAsync();
        var orderId = await connection.ExecuteScalarAsync<Guid>(
            "spSaveOrder",
            new { userId, productId, quantity, totalPrice },
            commandType: CommandType.StoredProcedure);
        return orderId;
    }
}

public class StandardPriceCalculator
{
    public int CalculateTotalPrice(int quantity, int price)
    {
        return price * quantity;
    }
}

public class NLogAdapter
{
    public void LogProductNotEnough(Guid productId, int quantity)
    {
        ILogger logger = new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger(this.GetType());
        logger.LogInformation(
            "商品{productId}數量少於{quantity}",
            productId, quantity);
    }
}

public class OrderService
{
    private readonly UserProxy _user = new UserProxy();
    private readonly ProductProxy _product = new ProductProxy();
    private readonly SmtpAdapter _notification = new SmtpAdapter();
    private readonly OrderDao _order = new OrderDao();
    private readonly StandardPriceCalculator _price = new StandardPriceCalculator();
    private readonly NLogAdapter _log = new NLogAdapter();

    public async Task<Guid> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity)
    {
        // check is valid user
        if (!await _user.IsUserValidAsync(userId))
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }

        // reserve product for the session
        var sessionId = await _product.ReserveProductAsync(productId, quantity);
        if (sessionId == Guid.Empty)
        {
            // api return empty guid when product is not enough
            _log.LogProductNotEnough(productId, quantity);
            throw new ProductNotEnoughException
            {
                ProductId = productId,
            };
        }

        // calculate total price
        var price = await _product.GetPriceAsync(productId);
        int totalPrice = _price.CalculateTotalPrice(quantity, price);

        // save order
        var orderId = await _order.SaveOrderAsync(userId, productId, quantity, totalPrice);

        // complete product reserve session
        await _product.CompleteProductReserveAsync(sessionId);

        // notify user for order established
        await _notification.NotifyUserAsync(orderId);

        return orderId;
    }
}
