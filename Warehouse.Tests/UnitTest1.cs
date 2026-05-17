namespace Warehouse.Tests;

using Warehouse.Models;
using Warehouse.Services;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

public class WarehouseTests
{
  
    private static (MainController controller, Mock<IWarehouseService> serviceMock) CreateController()
    {
        var serviceMock = new Mock<IWarehouseService>();
        var loggerMock = new Mock<ILogger<MainController>>();
        var controller = new MainController(serviceMock.Object, loggerMock.Object);
        return (controller, serviceMock);
    }

    // --- Model ---

    [Fact]
    public void CreateProduct_ShouldSetValues()
    {
        var product = new WarehouseItem
        {
            Name = "Apple",
            SKU = "A1",
            Category = "Food",
            Quantity = 10
        };

        Assert.Equal("Apple", product.Name);
        Assert.Equal("A1", product.SKU);
        Assert.Equal("Food", product.Category);
        Assert.Equal(10, product.Quantity);
    }

    // --- Add ---

    [Fact]
    public async Task Add_ShouldReturnOk_WithId()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.AddProductAsync(It.IsAny<WarehouseItem>()))
            .ReturnsAsync(42L);

        var result = await controller.Add(new WarehouseItem { Name = "Apple", SKU = "A1", Category = "Food", Quantity = 10 });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("42", ok.Value!.ToString());
    }

    // --- Delete ---

    [Fact]
    public async Task DeleteProduct_ShouldReturnOk_WhenProductExists()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.DeleteProductAsync(1))
            .ReturnsAsync(true);

        var result = await controller.DeleteProduct(1);

        Assert.IsType<OkObjectResult>(result);
        serviceMock.Verify(s => s.DeleteProductAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnNotFound_WhenProductMissing()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.DeleteProductAsync(99))
            .ReturnsAsync(false);

        var result = await controller.DeleteProduct(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // --- GetProduct ---

    [Fact]
    public async Task GetProduct_ShouldReturnOk_WhenProductExists()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.GetProductAsync(1))
            .ReturnsAsync(new Dictionary<string, string>
            {
                { "name", "Apple" },
                { "SKU", "A1" },
                { "Category", "Food" },
                { "Quantity", "10" }
            });

        var result = await controller.GetProduct(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnNotFound_WhenMissing()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.GetProductAsync(99))
            .ReturnsAsync((Dictionary<string, string>?)null);

        var result = await controller.GetProduct(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // --- GetAllProducts ---

    [Fact]
    public async Task GetAllProducts_ShouldReturnList_WhenProductsExist()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(new List<object> { new { Id = "1", Name = "Apple" } });

        var result = await controller.GetAllProducts();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<object>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnEmptyList_WhenNoProducts()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(new List<object>());

        var result = await controller.GetAllProducts();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<object>>(ok.Value);
        Assert.Empty(list);
    }

    // --- SearchProduct ---

    [Fact]
    public async Task SearchProduct_ShouldReturnOnlyMatchedItems()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.SearchProductsAsync("Appl"))
            .ReturnsAsync(new List<object> { new { Id = "1", Name = "Apple" } });

        var result = await controller.SearchProduct("Appl");

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<object>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task SearchProduct_ShouldReturnEmpty_WhenNoMatch()
    {
        var (controller, serviceMock) = CreateController();

        serviceMock
            .Setup(s => s.SearchProductsAsync("xyz"))
            .ReturnsAsync(new List<object>());

        var result = await controller.SearchProduct("xyz");

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<object>>(ok.Value);
        Assert.Empty(list);
    }
}