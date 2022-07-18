namespace DemoBuyingProduct;

public interface IUsers
{
    Task<bool> IsUserValidAsync(Guid userId);
}
