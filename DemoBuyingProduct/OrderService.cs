namespace DemoBuyingProduct;

public class OrderService : IOrderService
{
    public OrderService(ILogistic logistic, IOrders orders)
    {
        _logistic = logistic;
        _orders = orders;
    }

    private readonly ILogistic _logistic;
    private readonly IOrders _orders;

    public async Task<bool> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity)
    {
        var orderId = await _orders.SaveOrderAsync(userId, productId, quantity);

        await _logistic.NotifyLogisticForShippingProductAsync(orderId);

        return true;
    }
}
