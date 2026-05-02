using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Warehouse.Models;



[ApiController]
[Route("/")]
public class MainController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<MainController> _logger;

    public MainController(IConnectionMultiplexer redis, ILogger<MainController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    [HttpPost("add")]
    public async Task<IActionResult> add([FromBody] WarehouseItem product)
    {
        var db = _redis.GetDatabase();  // db 

        var id = await db.StringIncrementAsync("product:id");

        _logger.LogInformation("Adding new product: {@Product}", product);

        await db.HashSetAsync($"product:{id}",  new HashEntry[]
        {
        new HashEntry("name", product.Name),
        new HashEntry("SKU", product.SKU),
        new HashEntry("Category", product.Category),
        new HashEntry("Quantity", product.Quantity)
        });

        await db.ListRightPushAsync("product:list", id);
        
        _logger.LogInformation("Product added with ID: {ProductId}", id);

        return Ok($"The product has been saved");
    }


    [HttpGet("product/del/{id}")]
    public async Task<IActionResult> deleteProduct(long id)
    {
        
        var db = _redis.GetDatabase();
        bool deleted = await db.KeyDeleteAsync($"product:{id}");

        if (!deleted)
        {
            _logger.LogWarning("Attempted to delete non-existent product with ID: {ProductId}", id);
            return NotFound("The product doesn't exist");
        }
        await db.ListRemoveAsync("product:list", id);
        _logger.LogInformation("Product deleted with ID: {ProductId}", id);
        return Ok("The product has been deleted");

    }

    [HttpGet("product/{id}")]
    public async Task<IActionResult> getProduct(long id)
    {
        var db = _redis.GetDatabase();
        var data = await db.HashGetAllAsync($"product:{id}");
        if (data.Length == 0)
        {
            _logger.LogWarning("Product not found with ID: {ProductId}", id);
            return NotFound("The product doesn't exist");
        }
        _logger.LogInformation("Product {Id} retrieved successfully", id);
        var result = data.ToDictionary(
             a => a.Name.ToString(),
             a => a.Value.ToString()
             );

        return Ok(result);

    }

    [HttpGet("/products")]
    public async Task<IActionResult> getAllProducts()
    {
        var db = _redis.GetDatabase();
        var all = await db.ListRangeAsync("product:list");
        var products = new List<object>();
        foreach (var id in all)
        {
            var data = await db.HashGetAllAsync($"product:{id}");
            var product = new
            {
                Id = id.ToString(),
                Name = data.FirstOrDefault(a => a.Name == "name").Value.ToString(),
                SKU = data.FirstOrDefault(a => a.Name == "SKU").Value.ToString(),
                Category = data.FirstOrDefault(a => a.Name == "Category").Value.ToString(),
                Quantity = data.FirstOrDefault(a => a.Name == "Quantity").Value.ToString()
            };

             products.Add(product);
        }

        _logger.LogInformation("Returned {Count} products", products.Count);

        return Ok(products);
    }

    [HttpGet("/searchProduct")]
    public async Task<IActionResult> searchProduct(string word)
    {
        var db = _redis.GetDatabase();
        var ids = await db.ListRangeAsync("product:list");
        var products = new List<object>();
        foreach (var id in ids)
        {
            var data = await db.HashGetAllAsync($"product:{id}");

            if (data.Any(x => x.Value.ToString().Contains(word, StringComparison.OrdinalIgnoreCase)))
            {
                var product = new
                {
                    Id = id.ToString(),
                    Name = data.FirstOrDefault(a => a.Name == "name").Value.ToString(),
                    SKU = data.FirstOrDefault(a => a.Name == "SKU").Value.ToString(),
                    Category = data.FirstOrDefault(a => a.Name == "Category").Value.ToString(),
                    Quantity = data.FirstOrDefault(a => a.Name == "Quantity").Value.ToString()
                };
                products.Add(product);
            }

        }
        _logger.LogInformation("Search completed. Found {Count} products", products.Count);
        return Ok(products);

    }

}
