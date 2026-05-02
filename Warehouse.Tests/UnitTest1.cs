namespace Warehouse.Tests;

using Warehouse.Models;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class WarehouseTests
{
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


    [Fact]
    public async Task DeleteProduct_ShouldReturnOk_WhenKeyIsDeleted()
    {
        var dbMock = new Mock<IDatabase>();
        var redisMock = new Mock<IConnectionMultiplexer>();
        var loggerMock = new Mock<ILogger<MainController>>();

        redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                 .Returns(dbMock.Object);

        dbMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
              .ReturnsAsync(true);

        var controller = new MainController(redisMock.Object, loggerMock.Object);
        var result = await controller.deleteProduct(1);

        Assert.IsType<OkObjectResult>(result);
        dbMock.Verify(x => x.ListRemoveAsync("product:list", 1, 0, CommandFlags.None), Times.Once);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnList_WhenProductsExist()
    {

        var dbMock = new Mock<IDatabase>();
        var redisMock = new Mock<IConnectionMultiplexer>();
        var loggerMock = new Mock<ILogger<MainController>>();

        redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

        var redisIds = new RedisValue[] { "1" };
        dbMock.Setup(x => x.ListRangeAsync("product:list", 0, -1, CommandFlags.None))
              .ReturnsAsync(redisIds);

        var hashEntries = new HashEntry[]
        {
        new HashEntry("name", "Apple"),
        new HashEntry("SKU", "A1"),
        new HashEntry("Category", "Food"),
        new HashEntry("Quantity", "10")
        };
        dbMock.Setup(x => x.HashGetAllAsync("product:1", CommandFlags.None))
              .ReturnsAsync(hashEntries);

        var controller = new MainController(redisMock.Object, loggerMock.Object);

        var result = await controller.getAllProducts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<object>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task SearchProduct_ShouldReturnOnlyMatchedItems()
    {
        var dbMock = new Mock<IDatabase>();
        var redisMock = new Mock<IConnectionMultiplexer>();
        var loggerMock = new Mock<ILogger<MainController>>();

        redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

        dbMock.Setup(x => x.ListRangeAsync("product:list", 0, -1, It.IsAny<CommandFlags>()))
              .ReturnsAsync(new RedisValue[] { "1", "2" });

        dbMock.Setup(x => x.HashGetAllAsync("product:1", It.IsAny<CommandFlags>()))
              .ReturnsAsync(new HashEntry[] { new HashEntry("name", "Apple"), new HashEntry("Category", "Fruit") });

        dbMock.Setup(x => x.HashGetAllAsync("product:2", It.IsAny<CommandFlags>()))
              .ReturnsAsync(new HashEntry[] { new HashEntry("name", "Banana"), new HashEntry("Category", "Fruit") });

        var controller = new MainController(redisMock.Object, loggerMock.Object);

        var result = await controller.searchProduct("Appl");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsType<List<object>>(okResult.Value);

        Assert.Single(products);
    }
}