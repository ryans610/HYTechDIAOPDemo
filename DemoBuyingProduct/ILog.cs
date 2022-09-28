namespace DemoBuyingProduct;

public interface ILog
{
    void LogProductNotEnough(Guid productId, int quantity);
}