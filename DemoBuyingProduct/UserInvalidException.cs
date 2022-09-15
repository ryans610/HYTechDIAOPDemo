namespace DemoBuyingProduct;

public class UserInvalidException : Exception
{
    public Guid UserId { get; set; }
}