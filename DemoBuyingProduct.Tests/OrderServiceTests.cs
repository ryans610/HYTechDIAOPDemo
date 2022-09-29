namespace DemoBuyingProduct.Tests;

[TestFixture]
public class OrderServiceTests
{
    private readonly Guid _userId = new("{CA6B6DA3-26B2-455A-878E-AD7F15A0E6FF}");
    private readonly Guid _productId = new("{70C8A13C-99CA-44B4-8724-703AA8004076}");
    private readonly Guid _sessionId = new("{1E39ED75-9D7A-4FE8-A25B-732A3CA78423}");
    private readonly Guid _orderId = new("{EBDA0C8A-F360-4B20-AB27-7ACB379F1164}");

    private IUser _user;
    private IProduct _product;
    private INotification _notification;
    private IOrder _order;
    private IPriceCalculator _price;
    private ILog _log;

    private OrderService _orderService;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<IUser>();
        _product = Substitute.For<IProduct>();
        _notification = Substitute.For<INotification>();
        _order = Substitute.For<IOrder>();
        _price = new StandardPriceCalculator();
        _log = Substitute.For<ILog>();
        _orderService = new OrderService(_user, _product, _notification, _order, _price, _log);
    }

    [Test]
    public async Task Success()
    {
        _user.IsUserValidAsync(_userId).Returns(true);
        _product.ReserveProductAsync(_productId, Arg.Any<int>()).Returns(_sessionId);
        _product.GetPriceAsync(_productId).Returns(30);
        _order.SaveOrderAsync(_userId, _productId, Arg.Any<int>(), Arg.Any<int>()).Returns(_orderId);

        var result = await _orderService.OrderAsync(_userId, _productId, 5);

        await _product.Received(1).CompleteProductReserveAsync(_sessionId);
        await _notification.Received(1).NotifyUserAsync(_orderId);
        Assert.That(result, Is.EqualTo(_orderId));
    }
}
