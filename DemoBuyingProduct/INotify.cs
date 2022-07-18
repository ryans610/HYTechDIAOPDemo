namespace DemoBuyingProduct;

public interface INotify
{
    Task NotifyManagerAsync(string subject, string text);
}
