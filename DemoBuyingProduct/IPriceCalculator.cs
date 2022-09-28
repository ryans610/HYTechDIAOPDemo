namespace DemoBuyingProduct;

public interface IPriceCalculator
{
    int CalculateTotalPrice(int quantity, int price);
}