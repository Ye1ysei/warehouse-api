using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Warehouse.Models;
using Warehouse.Services;



[ApiController]
[Route("/")]
public class MainController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<MainController> _logger;

    public MainController(IWarehouseService warehouseService, ILogger<MainController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] WarehouseItem product)
    {
       var id = await _warehouseService.AddProductAsync(product);
        return Ok($"The product has been saved id:({id})");
    }


    [HttpGet("product/del/{id}")]
    public async Task<IActionResult> DeleteProduct(long id)
    {
        var deleted = await _warehouseService.DeleteProductAsync(id);
        if (!deleted) { return NotFound("The product doesn't exist"); }
        return Ok("The product has been deleted");

    }

    [HttpGet("product/{id}")]
    public async Task<IActionResult> GetProduct(long id)
    {
        var product = await _warehouseService.GetProductAsync(id);
        if (product == null) { return NotFound("The product doesn't exist"); }
        return Ok(product);

    }

    [HttpGet("/products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _warehouseService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("/searchProduct")]
    public async Task<IActionResult> SearchProduct(string products)
    {
        var product = await _warehouseService.SearchProductsAsync(products);
        return Ok(product);

    }

}
