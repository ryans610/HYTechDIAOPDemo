namespace DemoBuyingProduct;

public interface IProduct
{
    Task CompleteProductReserveAsync(Guid sessionId);
    Task<int> GetPriceAsync(Guid productId);
    Task<Guid> ReserveProductAsync(Guid productId, int quantity);
}