namespace DemoBuyingProduct;

public interface INotification
{
    Task NotifyUserAsync(Guid orderId);
}