using System.Net.Http;

namespace DemoBuyingProduct;

public class ProductProxy : IProduct
{
    public async Task CompleteProductReserveSessionAsync(Guid sessionId)
    {
        await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/complete",
            new { sessionId });
    }

    public async Task<int> GetProductPriceByIdAsync(Guid productId)
    {
        var productPriceResponse = await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/getPrice",
            new { productId });
        productPriceResponse.EnsureSuccessStatusCode();
        int price = await productPriceResponse.Content.ReadAsAsync<int>();
        return price;
    }

    public async Task<Guid> ReserveProductsAsync(Guid productId, int quantity)
    {
        var productReserveResponse = await new HttpClient
        {
            BaseAddress = new Uri("http://my-product"),
        }.PostAsJsonAsync(
            "api/reserve",
            new { productId, quantity });
        productReserveResponse.EnsureSuccessStatusCode();
        var sessionId = await productReserveResponse.Content.ReadAsAsync<Guid>();
        return sessionId;
    }
}