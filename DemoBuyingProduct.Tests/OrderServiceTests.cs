using System.Threading.Tasks;

namespace DemoBuyingProduct.Tests;

public class OrderServiceTests
{
    private ILogistic _logistic;
    private IOrders _orders;
    private INotify _notify;
    private IUsers _users;
    private OrderService _orderService;

    [SetUp]
    public void Setup()
    {
        _logistic = Substitute.For<ILogistic>();
        _orders = Substitute.For<IOrders>();
        _notify = Substitute.For<INotify>();
        _users = Substitute.For<IUsers>();

        _orderService = new OrderService(
            _logistic,
            _orders,
            _notify,
            _users);
    }

    [Test]
    public async Task OrderSuccess()
    {
        var userId = new Guid("{CA6B6DA3-26B2-455A-878E-AD7F15A0E6FF}");
        var productId = new Guid("{70C8A13C-99CA-44B4-8724-703AA8004076}");
        int quantity = 1;

        _users.IsUserValidAsync(userId).Returns(true);
        _orders.GetProductReserveAsync(productId).Returns(quantity);

        var isSuccess = await _orderService.OrderAsync(userId, productId, quantity);
        Assert.That(isSuccess, Is.EqualTo(true));
    }
}
