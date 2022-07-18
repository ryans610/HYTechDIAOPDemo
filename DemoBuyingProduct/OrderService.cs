namespace DemoBuyingProduct;

public class OrderService
{
    public OrderService(ILogistic logistic, IOrders orders, INotify notify, IUsers users)
    {
        _logistic = logistic;
        _orders = orders;
        _notify = notify;
        _users = users;
    }

    private readonly ILogistic _logistic;
    private readonly IOrders _orders;
    private readonly INotify _notify;
    private readonly IUsers _users;

    public async Task<bool> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity)
    {
        if (!await _users.IsUserValidAsync(userId))
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }

        // check reserve product is enough
        int reserve = await _orders.GetProductReserveAsync(productId);
        if (reserve < quantity)
        {
            await _notify.NotifyManagerAsync(
                "庫存不足",
                $"商品{productId}庫存不足");

            return false;
        }

        var orderId = await _orders.SaveOrderAsync(userId, productId, quantity);

        await _logistic.NotifyLogisticForShippingProductAsync(orderId);

        return true;
    }
}
