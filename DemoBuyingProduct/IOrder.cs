namespace DemoBuyingProduct;

public interface IOrder
{
    Task<Guid> SaveOrderAsync(Guid userId, Guid productId, int quantity, int totalPrice);
}