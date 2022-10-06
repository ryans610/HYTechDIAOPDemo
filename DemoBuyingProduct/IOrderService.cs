namespace DemoBuyingProduct;

public interface IOrderService
{
    Task<Guid> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity);
}
