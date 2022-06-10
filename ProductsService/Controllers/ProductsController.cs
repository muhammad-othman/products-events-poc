using Microsoft.AspNetCore.Mvc;
using ProductsService.Data;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsEventsStore productsService;

        public ProductsController(IProductsEventsStore productsService)
        {
            this.productsService = productsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await productsService.GetProducts();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Product product)
        {
            var addedProducts = await productsService.AddProducts(product);
            return Ok(addedProducts);
        }

        [HttpPut]
        public async Task<IActionResult> Put(Product product)
        {
            var updatedProducts = await productsService.UpdateProducts(product);
            return Ok(updatedProducts);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await productsService.DeleteProducts(id);
            return Ok();
        }

    }
}