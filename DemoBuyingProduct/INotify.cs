namespace DemoBuyingProduct;

public interface INotify
{
    Task NotifyOrderEstablishedAsync(Guid orderId);
}