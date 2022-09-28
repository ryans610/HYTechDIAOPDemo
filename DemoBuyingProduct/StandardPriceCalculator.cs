namespace DemoBuyingProduct;

public class StandardPriceCalculator : IPriceCalculator
{
    public int CalculateTotalPrice(int quantity, int price)
    {
        return price * quantity;
    }
}