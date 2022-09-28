namespace DemoBuyingProduct.Tests;

[TestFixture]
public class OrderServiceTests
{
    private readonly Guid _userId = new("{CA6B6DA3-26B2-455A-878E-AD7F15A0E6FF}");
    private readonly Guid _productId = new("{70C8A13C-99CA-44B4-8724-703AA8004076}");
    private readonly Guid _sessionId = new("{1E39ED75-9D7A-4FE8-A25B-732A3CA78423}");

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


}
