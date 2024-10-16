using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Models;
using ProductAPI.Services;
using System.Security.Cryptography;

namespace ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return Ok(await _productService.GetProducts());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {

            var product = await _productService.GetProductById(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateProduct(Product product)
        {
            var newproduct = await _productService.CreateProduct(product);
            //return Ok(newproduct);
            //return CreatedAtAction(nameof(GetProduct), new { id = newproduct }, product);

            var result = await GetProduct(newproduct);
            return result.Result;

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var deleteProduct = await _productService.DeleteProduct(id);
            if (!deleteProduct)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id,Product product)
        {
            if (id != product.Id)
                return BadRequest();
            var updateProduct = await _productService.UpdateProduct(product);
            if (!updateProduct)
                return NotFound();

            return NoContent();
        }

    }
}
