using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace DemoBuyingProduct;

public class OrderDao : IOrder
{
    public async Task<Guid> SaveOrderAsync(Guid userId, Guid productId, int quantity, int totalPrice)
    {
        await using var connection = new SqlConnection("my connection string");
        await connection.OpenAsync();
        var orderId = await connection.ExecuteScalarAsync<Guid>(
            "spSaveOrder",
            new { userId, productId, quantity, totalPrice },
            commandType: CommandType.StoredProcedure);
        return orderId;
    }
}