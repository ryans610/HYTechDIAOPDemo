namespace DemoBuyingProduct;

public interface IUser
{
    Task<bool> IsUserValidAsync(Guid userId);
}