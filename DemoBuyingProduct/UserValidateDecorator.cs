namespace DemoBuyingProduct;

public class UserValidateDecorator : IOrderService
{
    public UserValidateDecorator(IOrderService orderService, IUsers users)
    {
        _orderService = orderService;
        _users = users;
    }

    private readonly IOrderService _orderService;
    private readonly IUsers _users;

    public async Task<bool> OrderAsync(Guid userId, Guid productId, int quantity)
    {
        if (!await _users.IsUserValidAsync(userId))
        {
            throw new UserInvalidException
            {
                UserId = userId,
            };
        }
        return await _orderService.OrderAsync(userId, productId, quantity);
    }
}
