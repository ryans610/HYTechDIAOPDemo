namespace DemoBuyingProduct;

public interface IUserProxy
{
    Task<bool> IsUserValidAsync(Guid userId);
}
