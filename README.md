#  DAY1: Unit Testing In WebAPI C# xUnit Moq
 
# Introduction and Create Project
![image](https://github.com/user-attachments/assets/a8f4ce6e-74cf-4fab-a73d-39428ac8b2d6)

## 1. Create a new .NET 8 Web API project (ProductAPI).

![image](https://github.com/user-attachments/assets/7fdab0cf-a701-422d-a18a-04c496cd08c5)


## 2. Install the following NuGet packages:

* Dapper
* Microsoft.Data.SqlClient

* ![image](https://github.com/user-attachments/assets/eeb8ffc6-fece-49cc-aa8a-411ce2f7afaa)

 
## 3. Data Layer (Dapper):

* Create a Data folder.
* Inside Data, create Class DapperContext:
  
```csharp
// Data/DapperContext.cs
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ProductAPI.Data
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("LocaldbConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
```
## 4. Data Models 

* Create a Models folder
* Inside Models, create Class Product.cs
```csharp
// Models/Product.cs
namespace ProductAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```
```sql
CREATE TABLE [dbo].[Products] (
    [Id]    INT             NOT NULL IDENTITY,
    [Name]  NVARCHAR (80)   NULL,
    [Price] DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
```

## 5. Data Access Layer ... 
![image](https://github.com/user-attachments/assets/aa639a51-3b10-41a8-92ae-60370e57bb7c)


* Create a Repositories folder.
* Inside Repositories, create an interface IProductRepository:
  
```csharp
// Repositories/IProductRepository.cs
using ProductAPI.Models;

namespace ProductAPI.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProductById(int id);
        Task<int> CreateProduct(Product product);
        Task<bool> UpdateProduct(Product product);
        Task<bool> DeleteProduct(int id);
    }
}
```

* Inside Repositories, create class ProductRepository:
  
```csharp
// Repositories/ProductRepository.cs
using Dapper;
using ProductAPI.Data;
using ProductAPI.Models;

namespace ProductAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly DapperContext _context;

        public ProductRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<Product>("SELECT * FROM Products");
            }
        }


        public async Task<Product> GetProductById(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
            }
        }

        public async Task<int> CreateProduct(Product product)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteScalarAsync<int>(
                    "INSERT INTO Products (Name, Price) VALUES (@Name, @Price); SELECT SCOPE_IDENTITY();", product);
            }
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteAsync(
                    "UPDATE Products SET Name = @Name, Price = @Price WHERE Id = @Id", product) > 0;
            }
        }


        public async Task<bool> DeleteProduct(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", new { Id = id }) > 0;
            }
        }
    }
}

```
## 6. Service Layer:
![image](https://github.com/user-attachments/assets/3ac8d47f-172c-4007-a761-e51b54241967)


* Create a folder Services.
* Inside Services, create an interface IProductService:
* 
```csharp

// Services/IProductService.cs
using ProductAPI.Models;

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
}
```
* Create a class ProductService implementing IProductService:

```csharp
// Services/ProductService.cs
using ProductAPI.Models;
using ProductAPI.Repositories;

namespace ProductAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _productRepository.GetProducts();
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _productRepository.GetProductById(id);
        }

        public async Task<int> CreateProduct(Product product)
        {
            return await _productRepository.CreateProduct(product);

        }
        public async Task<bool> UpdateProduct(Product product)
        {
            return await _productRepository.UpdateProduct(product);
        }


        public async Task<bool> DeleteProduct(int id)
        {
            return await _productRepository.DeleteProduct(id);
        }
    }
}

```
## 7. Controller Layer:
![image](https://github.com/user-attachments/assets/79247523-c70d-4f56-b337-51cb7cc5be75)


* Inside Controllers, create ProductsController:

```csharp
// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Models;
using ProductAPI.Services;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productService.GetProducts();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateProduct(Product product)
        {
            var createdProductId = await _productService.CreateProduct(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProductId }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (!await _productService.UpdateProduct(product))
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!await _productService.DeleteProduct(id))
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
```
## 8. Web API Dependencies Register

```csharp
// Program.cs (Startup configuration)
using Microsoft.OpenApi.Models;
using ProductAPI.Data;
using ProductAPI.Repositories;
using ProductAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductAPI", Version = "v1" });
});

builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductAPI v1"));

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

```

## Add Project UnitTest
![AddUnitTestProject](https://github.com/user-attachments/assets/88a71f22-8ceb-465f-a0a2-c316c00681c7)

