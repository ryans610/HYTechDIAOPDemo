namespace DemoBuyingProduct;

public class OrderService
{
    public OrderService(
        IUser user,
        IProduct product,
        INotification notification,
        IOrder order,
        IPriceCalculator priceCalculator,
        ILog log)
    {
        _user = user;
        _product = product;
        _notification = notification;
        _order = order;
        _price = priceCalculator;
        _log = log;
    }

    private readonly IUser _user;
    private readonly IProduct _product;
    private readonly INotification _notification;
    private readonly IOrder _order;
    private readonly IPriceCalculator _price;
    private readonly ILog _log;

    public async Task<Guid> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity)
    {
        // check is valid user
        if (!await _user.IsUserValidAsync(userId))
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }

        // reserve product for the session
        var sessionId = await _product.ReserveProductAsync(productId, quantity);
        if (sessionId == Guid.Empty)
        {
            // api return empty guid when product is not enough
            _log.LogProductNotEnough(productId, quantity);
            throw new ProductNotEnoughException
            {
                ProductId = productId,
            };
        }

        // calculate total price
        var price = await _product.GetPriceAsync(productId);
        int totalPrice = _price.CalculateTotalPrice(quantity, price);

        // save order
        var orderId = await _order.SaveOrderAsync(userId, productId, quantity, totalPrice);

        // complete product reserve session
        await _product.CompleteProductReserveAsync(sessionId);

        // notify user for order established
        await _notification.NotifyUserAsync(orderId);

        return orderId;
    }
}
