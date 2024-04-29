using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CartMicroservice
{
    public static class CartEndpoints
    {
        public static void MapCartEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/cart").RequireAuthorization();

            group.MapGet("", GetCart);

            group.MapPost("", CreateCart);

            group.MapPut("", EditCart);

            group.MapDelete("", DeleteCart);

            group.MapPut("/update-catalog-item", UpdateCatalogItem);

            group.MapDelete("/delete-catalog-item", DeleteCatalogItem);
        }

        public static async Task<Ok<IList<CartItem>>> GetCart([FromQuery(Name = "u")] string userId,
                ICartRepository cartRepository)
        {
            var cartItems = cartRepository.GetCartItems(userId);
            return TypedResults.Ok(cartItems);
        }

        public static async Task<Ok> CreateCart([FromQuery(Name = "u")] string userId,
                                  [FromBody] CartItem cartItem,
                                  ICartRepository cartRepository)
        {
            cartRepository.InsertCartItem(userId, cartItem);
            return TypedResults.Ok();
        }

        public static async Task<Ok> EditCart([FromQuery(Name = "u")] string userId,
                                  [FromBody] CartItem cartItem,
                                  ICartRepository cartRepository)
        {
            cartRepository.UpdateCartItem(userId, cartItem);
            return TypedResults.Ok();
        }

        public static async Task<Ok> DeleteCart([FromQuery(Name = "u")] string userId,
                      [FromQuery(Name = "ci")] string cartItemId,
                      ICartRepository cartRepository)
        {
            cartRepository.DeleteCartItem(userId, cartItemId);
            return TypedResults.Ok();
        }

        public static async Task<Ok> UpdateCatalogItem([FromQuery(Name = "ci")] string catalogItemId,
                        [FromQuery(Name = "n")] string name,
                        [FromQuery(Name = "p")] decimal price,
                        ICartRepository cartRepository)
        {
            cartRepository.UpdateCatalogItem(catalogItemId, name, price);
            return TypedResults.Ok();
        }

        public static async Task<Ok> DeleteCatalogItem([FromQuery(Name = "ci")] string catalogItemId, ICartRepository cartRepository)
        {
            cartRepository.DeleteCatalogItem(catalogItemId);
            return TypedResults.Ok();
        }
    }
}
