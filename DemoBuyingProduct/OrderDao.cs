using System.Data;
using System.Data.SqlClient;

using Dapper;

namespace DemoBuyingProduct;

public class OrderDao : IOrders
{
    public async Task<Guid> SaveOrderAsync(Guid userId, Guid productId, int quantity)
    {
        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<Guid>(
            "spSaveOrder",
            new { userId, productId, quantity },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetProductReserveAsync(Guid productId)
    {
        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "spGetProductReserveCount",
            new { productId },
            commandType: CommandType.StoredProcedure);
    }

    private async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection("my connection string");
        await connection.OpenAsync();
        return connection;
    }
}
