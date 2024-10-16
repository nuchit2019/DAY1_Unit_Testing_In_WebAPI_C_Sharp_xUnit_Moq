using ProductAPI.Models;
using ProductAPI.Repositories;

namespace ProductAPI.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProductById(int id);
        Task<int> CreateProduct(Product product);
        Task<bool> UpdateProduct(Product product);
        Task<bool> DeleteProduct(int id);
    }


    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
              
        }
        public async Task<int> CreateProduct(Product product)
        {
            return await _productRepository.CreateProduct(product);
        }

        public async Task<bool> DeleteProduct(int id)
        {
            return await _productRepository.DeleteProduct(id);
        }

        public async Task<Product> GetProductById(int id)
        {
            var product = await _productRepository.GetProductById(id);
            return   product;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
           return await _productRepository.GetProducts();
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            return await _productRepository.UpdateProduct(product);
        }
    }


}
