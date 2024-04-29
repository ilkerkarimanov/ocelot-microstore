using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogMicroservice
{
    public class CatalogEndpoints : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/catalog");

            group.MapGet("", GetCatalogItems).RequireAuthorization();

            group.MapGet("{id}", GetCatalogItem);

            group.MapPost("", CreateCatalogItem);

            group.MapPut("", UpdateCatalogItem);

            group.MapDelete("", DeleteCatalogItem);
        }

        public static async Task<Ok<IList<CatalogItem>>> GetCatalogItems(ICatalogRepository catalogRepository)
        {
            var catalogItems = catalogRepository.GetCatalogItems();
            return TypedResults.Ok(catalogItems);
        }

        public static async Task<Ok<CatalogItem>> GetCatalogItem(string id, ICatalogRepository catalogRepository)
        {
            var catalogItem = catalogRepository.GetCatalogItem(id);
            return TypedResults.Ok(catalogItem);
        }

        public static async Task<CreatedAtRoute<CatalogItem>> CreateCatalogItem([FromBody] CatalogItem catalogItem, ICatalogRepository catalogRepository)
        {
            catalogRepository.InsertCatalogItem(catalogItem);
            object routeParams = new { id = catalogItem.Id };
            return TypedResults.CreatedAtRoute(value: catalogItem, routeName: nameof(GetCatalogItem), routeValues: routeParams);
        }

        public static async Task<Results<Ok, NoContent>> UpdateCatalogItem([FromBody] CatalogItem? catalogItem, ICatalogRepository catalogRepository)
        {
            if (catalogItem != null)
            {
                catalogRepository.UpdateCatalogItem(catalogItem);
                return TypedResults.Ok();
            }
            return TypedResults.NoContent();
        }

        public static async Task<Ok> DeleteCatalogItem(string id, ICatalogRepository catalogRepository)
        {
            catalogRepository.DeleteCatalogItem(id);
            return TypedResults.Ok();
        }
    }
}
