namespace DemoBuyingProduct;

public interface ILogistic
{
    Task NotifyLogisticForShippingProductAsync(Guid orderId);
}
