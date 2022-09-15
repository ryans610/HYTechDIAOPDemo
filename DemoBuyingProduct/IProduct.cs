namespace DemoBuyingProduct;

public interface IProduct
{
    Task CompleteProductReserveSessionAsync(Guid sessionId);
    Task<int> GetProductPriceByIdAsync(Guid productId);
    Task<Guid> ReserveProductsAsync(Guid productId, int quantity);
}