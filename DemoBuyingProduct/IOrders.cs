namespace DemoBuyingProduct;

public interface IOrders
{
    Task<int> GetProductReserveAsync(Guid productId);
    Task<Guid> SaveOrderAsync(Guid userId, Guid productId, int quantity);
}
