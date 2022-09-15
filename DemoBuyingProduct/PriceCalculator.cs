namespace DemoBuyingProduct;

public class PriceCalculator : IPriceCalculator
{
    public PriceCalculator(IProduct product)
    {
        _product = product;
    }

    private readonly IProduct _product;

    public async Task<int> CalculateTotalPriceAsync(Guid productId, int quantity)
    {
        var price = await _product.GetProductPriceByIdAsync(productId);
        int totalPrice = price * quantity;
        return totalPrice;
    }
}