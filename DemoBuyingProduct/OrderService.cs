namespace DemoBuyingProduct;

public class OrderService
{
    public OrderService(
        IUserProxy user,
        ILog log,
        IOrder order,
        INotify notify,
        IPriceCalculator priceCalculator,
        IProduct product)
    {
        _user = user;
        _log = log;
        _order = order;
        _notify = notify;
        _priceCalculator = priceCalculator;
        _product = product;
    }

    private readonly IUserProxy _user;
    private readonly ILog _log;
    private readonly IOrder _order;
    private readonly INotify _notify;
    private readonly IPriceCalculator _priceCalculator;
    private readonly IProduct _product;

    public async Task<bool> OrderAsync(
        Guid userId,
        Guid productId,
        int quantity)
    {
        // check is valid user
        if (await _user.IsUserValidAsync(userId))
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }

        // reserve product for the session
        var sessionId = await _product.ReserveProductsAsync(productId, quantity);
        if (sessionId == Guid.Empty)
        {
            // api return empty guid when product is not enough
            _log.LogProductNotEnough(productId, quantity);

            return false;
        }

        // calculate total price
        var totalPrice = await _priceCalculator.CalculateTotalPriceAsync(productId, quantity);

        // save order
        var orderId = await _order.SaveOrderAsync(userId, productId, quantity, totalPrice);

        // complete product reserve session
        await _product.CompleteProductReserveSessionAsync(sessionId);

        // notify user for order established
        await _notify.NotifyOrderEstablishedAsync(orderId);

        return true;
    }
}
