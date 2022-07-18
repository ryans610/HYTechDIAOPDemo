using System.Net.Http;

namespace DemoBuyingProduct;

public class LogisticProxy : ILogistic
{
    public async Task NotifyLogisticForShippingProductAsync(Guid orderId)
    {
        var logisticHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://my-logistic"),
        };
        var logisticResponse = await logisticHttpClient.PostAsJsonAsync(
            "api/shipping",
            new { orderId });
        logisticResponse.EnsureSuccessStatusCode();
    }
}
