using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CartMicroservice.Test
{
    public class CartEndpointsTest
    {
        private readonly ICartRepository _repository;
        private static readonly string UserId = "653e43b8c76b6b56a720803e";
        private static readonly string A54Id = "653e4410614d711b7fc953a7";
        private static readonly string A14Id = "253e4410614d711b7fc953a7";
        private readonly Dictionary<string, List<CartItem>> _carts = new()
        {
            {
                UserId,
                new()
                {
                    new()
                    {
                        CatalogItemId = A54Id,
                        Name = "Samsung Galaxy A54 5G",
                        Price = 500,
                        Quantity = 1
                    },
                    new()
                    {
                        CatalogItemId = A14Id,
                        Name = "Samsung Galaxy A14 5G",
                        Price = 200,
                        Quantity = 2
                    }
                }
            }
        };

        public CartEndpointsTest()
        {
            var mockRepo = new Mock<ICartRepository>();
            mockRepo.Setup(repo => repo.GetCartItems(It.IsAny<string>()))
                .Returns<string>(id => _carts[id]);
            mockRepo.Setup(repo => repo.InsertCartItem(It.IsAny<string>(),
                                   It.IsAny<CartItem>()))
                .Callback<string, CartItem>((userId, item) =>
                {
                    if (_carts.TryGetValue(userId, out var items))
                    {
                        items.Add(item);
                    }
                    else
                    {
                        _carts.Add(userId, new List<CartItem> { item });
                    }
                });
            mockRepo.Setup(repo => repo.UpdateCartItem(It.IsAny<string>(),
                                   It.IsAny<CartItem>()))
                .Callback<string, CartItem>((userId, item) =>
                {
                    if (_carts.TryGetValue(userId, out var items))
                    {
                        var currentItem = items.FirstOrDefault
                            (i => i.CatalogItemId == item.CatalogItemId);
                        if (currentItem != null)
                        {
                            currentItem.Name = item.Name;
                            currentItem.Price = item.Price;
                            currentItem.Quantity = item.Quantity;
                        }
                    }
                });
            mockRepo.Setup(repo => repo.UpdateCatalogItem
                     (It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Callback<string, string, decimal>((catalogItemId, name, price) =>
                {
                    var cartItems = _carts
                    .Values
                    .Where(items => items.Any(i => i.CatalogItemId == catalogItemId))
                    .SelectMany(items => items)
                    .ToList();

                    foreach (var cartItem in cartItems)
                    {
                        cartItem.Name = name;
                        cartItem.Price = price;
                    }
                });
            mockRepo.Setup(repo => repo.DeleteCartItem
                          (It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((userId, catalogItemId) =>
                {
                    if (_carts.TryGetValue(userId, out var items))
                    {
                        items.RemoveAll(i => i.CatalogItemId == catalogItemId);
                    }
                });
            mockRepo.Setup(repo => repo.DeleteCatalogItem(It.IsAny<string>()))
                .Callback<string>((catalogItemId) =>
                {
                    foreach (var cart in _carts)
                    {
                        cart.Value.RemoveAll(i => i.CatalogItemId == catalogItemId);
                    }
                });

            _repository = mockRepo.Object;
        }

        [Fact]
        public async Task GetCartItemsTest()
        {
            var okObjectResult = await CartEndpoints.GetCart(UserId, _repository);
            var okResult = Assert.IsType<Ok<IList<CartItem>>>(okObjectResult);
            var items = Assert.IsType<List<CartItem>>(okResult.Value);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task InsertCartItemTest()
        {
            var okObjectResult = await CartEndpoints.CreateCart(
                UserId,
                new CartItem
                {
                    CatalogItemId = A54Id,
                    Name = "Samsung Galaxy A54 5G",
                    Price = 500,
                    Quantity = 1
                },
                _repository
            );
            Assert.IsType<Ok>(okObjectResult);
            Assert.NotNull(_carts[UserId].FirstOrDefault(i => i.CatalogItemId == A54Id));
        }

        [Fact]
        public async Task UpdateCartItemTest()
        {
            var catalogItemId = A54Id;
            var okObjectResult = await CartEndpoints.EditCart(
                UserId,
                new CartItem
                {
                    CatalogItemId = A54Id,
                    Name = "Samsung Galaxy A54",
                    Price = 550,
                    Quantity = 2
                },
                _repository
            );
            Assert.IsType<Ok>(okObjectResult);
            var catalogItem = _carts[UserId].FirstOrDefault
                              (i => i.CatalogItemId == catalogItemId);
            Assert.NotNull(catalogItem);
            Assert.Equal("Samsung Galaxy A54", catalogItem.Name);
            Assert.Equal(550, catalogItem.Price);
            Assert.Equal(2, catalogItem.Quantity);
        }

        [Fact]
        public async Task DeleteCartItemTest()
        {
            var id = A14Id;
            var items = _carts[UserId];
            var item = items.FirstOrDefault(i => i.CatalogItemId == id);
            Assert.NotNull(item);
            var okObjectResult = await CartEndpoints.DeleteCart(UserId, id, _repository);
            Assert.IsType<Ok>(okObjectResult);
            item = items.FirstOrDefault(i => i.CatalogItemId == id);
            Assert.Null(item);
        }

        [Fact]
        public async Task UpdateCatalogItemTest()
        {
            var catalogItemId = A54Id;
            var okObjectResult = await CartEndpoints.UpdateCatalogItem(
                A54Id,
                "Samsung Galaxy A54",
                550,
                _repository
            );
            Assert.IsType<Ok>(okObjectResult);
            var catalogItem = _carts[UserId].FirstOrDefault
                             (i => i.CatalogItemId == catalogItemId);
            Assert.NotNull(catalogItem);
            Assert.Equal("Samsung Galaxy A54", catalogItem.Name);
            Assert.Equal(550, catalogItem.Price);
            Assert.Equal(1, catalogItem.Quantity);
        }

        [Fact]
        public async Task DeleteCatalogItemTest()
        {
            var id = A14Id;
            var items = _carts[UserId];
            var item = items.FirstOrDefault(i => i.CatalogItemId == id);
            Assert.NotNull(item);
            var okObjectResult = await CartEndpoints.DeleteCatalogItem(id, _repository);
            Assert.IsType<Ok>(okObjectResult);
            item = items.FirstOrDefault(i => i.CatalogItemId == id);
            Assert.Null(item);
        }
    }
}
