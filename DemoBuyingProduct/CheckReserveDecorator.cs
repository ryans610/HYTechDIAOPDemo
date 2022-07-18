namespace DemoBuyingProduct;

public class CheckReserveDecorator : IOrderService
{
    public CheckReserveDecorator(IOrderService orderService, IOrders orders, INotify notify)
    {
        _orderService = orderService;
        _orders = orders;
        _notify = notify;
    }

    private readonly IOrderService _orderService;
    private readonly IOrders _orders;
    private readonly INotify _notify;

    public async Task<bool> OrderAsync(Guid userId, Guid productId, int quantity)
    {
        int reserve = await _orders.GetProductReserveAsync(productId);
        if (reserve < quantity)
        {
            await _notify.NotifyManagerAsync(
                "庫存不足",
                $"商品{productId}庫存不足");
            return false;
        }
        return await _orderService.OrderAsync(userId, productId, quantity);
    }
}
