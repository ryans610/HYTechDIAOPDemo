using Microsoft.Extensions.DependencyInjection;

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

    private IOrderService _orderService;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection()
            .AddSingleton(Substitute.For<IUser>())
            .AddSingleton(Substitute.For<IProduct>())
            .AddSingleton(Substitute.For<INotification>())
            .AddSingleton(Substitute.For<IOrder>())
            .AddSingleton<IPriceCalculator>(new StandardPriceCalculator())
            .AddSingleton(Substitute.For<ILog>())
            .AddSingleton<IOrderService, OrderService>()
            .BuildServiceProvider();

        _user = services.GetRequiredService<IUser>();
        _product = services.GetRequiredService<IProduct>();
        _notification = services.GetRequiredService<INotification>();
        _order = services.GetRequiredService<IOrder>();
        _price = services.GetRequiredService<IPriceCalculator>();
        _log = services.GetRequiredService<ILog>();
        _orderService = services.GetRequiredService<IOrderService>();
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

    [Test]
    public void UserNotValid()
    {
        _user.IsUserValidAsync(_userId).Returns(false);

        var exception = Assert.ThrowsAsync<UserInvalidException>(
            () => _orderService.OrderAsync(_userId, _productId, 5));
        Assert.That(exception?.UserId, Is.EqualTo(_userId));
    }

    [Test]
    public void ProductNotEnough()
    {
        _user.IsUserValidAsync(_userId).Returns(true);
        _product.ReserveProductAsync(_productId, Arg.Any<int>()).Returns(Guid.Empty);

        var exception = Assert.ThrowsAsync<ProductNotEnoughException>(
            () => _orderService.OrderAsync(_userId, _productId, 5));
        Assert.That(exception?.ProductId, Is.EqualTo(_productId));
    }
}
