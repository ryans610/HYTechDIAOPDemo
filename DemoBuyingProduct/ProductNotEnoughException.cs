namespace DemoBuyingProduct;

public class ProductNotEnoughException : Exception
{
    public Guid ProductId { get; set; }
}
