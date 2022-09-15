namespace DemoBuyingProduct;

public class NLogAdapter : ILog
{
    public void LogProductNotEnough(Guid productId, int quantity)
    {
        ILogger logger = new NLog.Extensions.Logging.NLogLoggerFactory().CreateLogger(this.GetType());
        logger.LogInformation(
            "商品{productId}數量少於{quantity}",
            productId, quantity);
    }
}
