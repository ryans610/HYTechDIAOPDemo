namespace DemoBuyingProduct;

public interface IOrderService
{
    Task<bool> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity);
}
