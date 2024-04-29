using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogMicroservice.Test;

public class CatalogEndpointsTest
{
    private readonly ICatalogRepository _repository;
    private static readonly string A54Id = "653e4410614d711b7fc953a7";
    private static readonly string A14Id = "253e4410614d711b7fc953a7";
    private readonly List<CatalogItem> _items = new()
    {
        new()
        {
            Id = A54Id,
            Name = "Samsung Galaxy A54 5G",
            Description = "Samsung Galaxy A54 5G mobile phone",
            Price = 500
        },
        new()
        {
            Id = A14Id,
            Name = "Samsung Galaxy A14 5G",
            Description = "Samsung Galaxy A14 5G mobile phone",
            Price = 200
        }
    };

    public CatalogEndpointsTest()
    {
        var mockRepo = new Mock<ICatalogRepository>();
        mockRepo.Setup(repo => repo.GetCatalogItems()).Returns(_items);
        mockRepo.Setup(repo => repo.GetCatalogItem(It.IsAny<string>()))
            .Returns<string>(id => _items.FirstOrDefault(i => i.Id == id));
        mockRepo.Setup(repo => repo.InsertCatalogItem(It.IsAny<CatalogItem>()))
            .Callback<CatalogItem>(_items.Add);
        mockRepo.Setup(repo => repo.UpdateCatalogItem(It.IsAny<CatalogItem>()))
            .Callback<CatalogItem>(i =>
            {
                var item = _items.FirstOrDefault(catalogItem => catalogItem.Id == i.Id);
                if (item != null)
                {
                    item.Name = i.Name;
                    item.Description = i.Description;
                    item.Price = i.Price;
                }
            });
        mockRepo.Setup(repo => repo.DeleteCatalogItem(It.IsAny<string>()))
            .Callback<string>(id => _items.RemoveAll(i => i.Id == id));
        _repository = mockRepo.Object;
    }

    [Fact]
    public async Task GetCatalogItemsTest()
    {
        var okObjectResult = await CatalogEndpoints.GetCatalogItems(_repository);
        var okResult = Assert.IsType<Ok<IList<CatalogItem>>>(okObjectResult);
        var items = Assert.IsType<List<CatalogItem>>(okResult.Value);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetCatalogItemTest()
    {
        var id = A54Id;
        var okObjectResult = await CatalogEndpoints.GetCatalogItem(id, _repository);
        var okResult = Assert.IsType<Ok<CatalogItem>>(okObjectResult);
        var item = Assert.IsType<CatalogItem>(okResult.Value);
        Assert.Equal(id, item.Id);
    }

    [Fact]
    public async Task InsertCatalogItemTest()
    {
        var createdResponse = await CatalogEndpoints.CreateCatalogItem(
            new CatalogItem
            {
                Id = "353e4410614d711b7fc953a7",
                Name = "iPhone 15",
                Description = "iPhone 15 mobile phone",
                Price = 1500
            },
            _repository
        );
        var response = Assert.IsType<CreatedAtRoute<CatalogItem>>(createdResponse);
        var item = Assert.IsType<CatalogItem>(response.Value);
        Assert.Equal("iPhone 15", item.Name);
    }

    [Fact]
    public async Task UpdateCatalogItemTest()
    {
        var id = A54Id;
        var okObjectResult = await CatalogEndpoints.UpdateCatalogItem(
            new CatalogItem
            {
                Id = id,
                Name = "Samsung Galaxy S23 Ultra",
                Description = "Samsung Galaxy S23 Ultra mobile phone",
                Price = 1500
            },
            _repository);
        Assert.IsType<Results<Ok, NoContent>>(okObjectResult);
        var okResult = (Ok)okObjectResult.Result;
        Assert.IsType<Ok>(okResult);
        var item = _items.FirstOrDefault(i => i.Id == id);
        Assert.NotNull(item);
        Assert.Equal("Samsung Galaxy S23 Ultra", item.Name);
        okObjectResult = await CatalogEndpoints.UpdateCatalogItem(null, _repository);
        var noContentResult = (NoContent)okObjectResult.Result;
        Assert.IsType<NoContent>(noContentResult);
    }

    [Fact]
    public async Task DeleteCatalogItemTest()
    {
        var id = A54Id;
        var item = _items.FirstOrDefault(i => i.Id == id);
        Assert.NotNull(item);
        var okObjectResult = await CatalogEndpoints.DeleteCatalogItem(id, _repository);
        Assert.IsType<Ok>(okObjectResult);
        item = _items.FirstOrDefault(i => i.Id == id);
        Assert.Null(item);
    }
}