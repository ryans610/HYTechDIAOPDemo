namespace DemoBuyingProduct;

public interface IPriceCalculator
{
    Task<int> CalculateTotalPriceAsync(Guid productId, int quantity);
}