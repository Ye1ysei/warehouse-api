using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Warehouse.Models;
using Warehouse.Services;


namespace Warehouse.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<WarehouseService> _logger;

        public WarehouseService(IConnectionMultiplexer redis, ILogger<WarehouseService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<long> AddProductAsync(WarehouseItem product)
        {
            var db = _redis.GetDatabase();  // db 
            var id = await db.StringIncrementAsync("product:id");


            await db.HashSetAsync($"product:{id}", new HashEntry[]
            {
                new HashEntry("name", product.Name),
                new HashEntry("SKU", product.SKU),
                new HashEntry("Category", product.Category),
                new HashEntry("Quantity", product.Quantity)
            });

            await db.ListRightPushAsync("product:list", id);

            return id;
        }

        public async Task<bool> DeleteProductAsync(long id)
        {

            var db = _redis.GetDatabase();
            bool deleted = await db.KeyDeleteAsync($"product:{id}");

            if (deleted)
            {
                await db.ListRemoveAsync("product:list", id);
            }

            return deleted;

        }

        public async Task<Dictionary<string, string>?> GetProductAsync(long id)
        {
            var db = _redis.GetDatabase();
            var data = await db.HashGetAllAsync($"product:{id}");
            if (data.Length == 0)
            {
                return null;
            }

            return data.ToDictionary(a => a.Name.ToString(), a => a.Value.ToString());

        }

        public async Task<List<object>> GetAllProductsAsync()
        {
            var db = _redis.GetDatabase();
            var ids = await db.ListRangeAsync("product:list");
            var products = new List<object>();
            foreach (var id in ids)
            {
                var data = await db.HashGetAllAsync($"product:{id}");
                products.Add(MapToProduct(id.ToString(), data));
            }

            return products;
        }

        public async Task<List<object>> SearchProductsAsync(string word)
        {
            var db = _redis.GetDatabase();
            var ids = await db.ListRangeAsync("product:list");
            var products = new List<object>();
            foreach (var id in ids)
            {
                var data = await db.HashGetAllAsync($"product:{id}");

                if (data.Any(x => x.Value.ToString().Contains(word, StringComparison.OrdinalIgnoreCase)))
                {
                    products.Add(MapToProduct(id.ToString(), data));
                }

            }
            return products;
        }

        private static object MapToProduct(string id, HashEntry[] data) => new
        {
            Id = id,
            Name = data.FirstOrDefault(a => a.Name == "name").Value.ToString(),
            SKU = data.FirstOrDefault(a => a.Name == "SKU").Value.ToString(),
            Category = data.FirstOrDefault(a => a.Name == "Category").Value.ToString(),
            Quantity = data.FirstOrDefault(a => a.Name == "Quantity").Value.ToString()
        };

    }

}


