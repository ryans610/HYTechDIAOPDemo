namespace DemoBuyingProduct.Tests;

public class OrderServiceTests
{
    private OrderService _orderService;

    [SetUp]
    public void Setup()
    {
        _orderService = new OrderService(
            new LogisticProxy(),
            new OrderDao(),
            new EmailAdapter(),
            new UserProxy());
    }


}
