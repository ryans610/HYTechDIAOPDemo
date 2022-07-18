using System.Threading.Tasks;

using NSubstitute.ReceivedExtensions;

namespace DemoBuyingProduct.Tests;

public class OrderServiceTests
{
    private readonly Guid _userId = new("{CA6B6DA3-26B2-455A-878E-AD7F15A0E6FF}");
    private readonly Guid _productId = new("{70C8A13C-99CA-44B4-8724-703AA8004076}");

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
        GivenUserIsValid();
        GivenProductReserve(1);

        await ShouldBeSuccess(1);
    }

    [Test]
    public async Task NotifyLogisticForShippingProductWhenSuccess()
    {
        var orderId = Guid.NewGuid();
        await WhenSuccess(orderId);

        await ShouldNotifyLogistic(orderId);
    }

    [Test]
    public void ThrowOnUserInvalid()
    {
        GivenUserIsInvalid();

        ShouldThrow<UserInvalidException>();
    }

    [Test]
    public async Task ShouldFailWhenReserveNotEnough()
    {
        GivenUserIsValid();
        GivenProductReserve(1);

        await ShouldFailed(2);
    }

    [Test]
    public async Task ShouldNotifyManagerWhenReserveNotEnough()
    {
        GivenUserIsValid();
        GivenProductReserve(1);
        await GivenOrderQuantity(2);

        await _notify.Received(1).NotifyManagerAsync("庫存不足", $"商品{_productId}庫存不足");
    }

    private async Task WhenSuccess(Guid orderId)
    {
        GivenUserIsValid();
        GivenProductReserve(1);
        GivenOrderId(orderId);
        await GivenOrderQuantity(1);
    }

    private void GivenUserIsValid()
    {
        _users.IsUserValidAsync(_userId).Returns(true);
    }

    private void GivenUserIsInvalid()
    {
        _users.IsUserValidAsync(_userId).Returns(false);
    }

    private void GivenProductReserve(int quantity)
    {
        _orders.GetProductReserveAsync(_productId).Returns(quantity);
    }

    private async Task GivenOrderQuantity(int quantity)
    {
        await _orderService.OrderAsync(_userId, _productId, quantity);
    }

    private void GivenOrderId(Guid orderId)
    {
        _orders.SaveOrderAsync(_userId, _productId, 1).Returns(orderId);
    }

    private async Task ShouldBeSuccess(int quantity)
    {
        var isSuccess = await _orderService.OrderAsync(_userId, _productId, quantity);
        Assert.That(isSuccess, Is.EqualTo(true));
    }

    private async Task ShouldFailed(int quantity)
    {
        var isSuccess = await _orderService.OrderAsync(_userId, _productId, quantity);
        Assert.That(isSuccess, Is.EqualTo(false));
    }

    private async Task ShouldNotifyLogistic(Guid orderId)
    {
        await _logistic.Received(1).NotifyLogisticForShippingProductAsync(orderId);
    }

    private void ShouldThrow<TException>()
        where TException : Exception
    {
        Assert.ThrowsAsync<TException>(
            () => _orderService.OrderAsync(_userId, _productId, 1));
    }
}
