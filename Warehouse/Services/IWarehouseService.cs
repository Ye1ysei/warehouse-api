using Warehouse.Models;

namespace Warehouse.Services
{
    public interface IWarehouseService
    {
        Task<long> AddProductAsync(WarehouseItem product);
        Task<bool> DeleteProductAsync(long id);
        Task<Dictionary<string, string>?> GetProductAsync(long id);
        Task<List<object>> GetAllProductsAsync();
        Task<List<object>> SearchProductsAsync(string word);
    }
}
