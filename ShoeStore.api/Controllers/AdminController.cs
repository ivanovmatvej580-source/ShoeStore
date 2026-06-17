using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ShoeStore.api.Data;
using System.Linq;

namespace ShoeStore.api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        using var connection = new SqliteConnection(DbConfig.ConnectionString);
        var products = connection.Query(@"
            SELECT 
                id AS Id,
                name AS Name,
                brand AS Brand,
                category AS Category,
                CASE WHEN discount_price IS NOT NULL THEN discount_price ELSE price END AS CurrentPrice,
                price AS OriginalPrice,
                discount_price AS DiscountPrice,
                color AS Color,
                stock_quantity AS StockQuantity
            FROM products 
            WHERE is_active = 1
            ORDER BY id");

        return Ok(products);
    }

    [HttpPost("products")]
    public IActionResult AddProduct([FromBody] ProductRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Название обязательно" });

        using var connection = new SqliteConnection(DbConfig.ConnectionString);
        var id = connection.ExecuteScalar<int>(@"
            INSERT INTO products (name, brand, category, price, discount_price, color, stock_quantity) 
            VALUES (@Name, @Brand, @Category, @Price, @DiscountPrice, @Color, @StockQuantity);
            SELECT last_insert_rowid();",
            new
            {
                request.Name,
                Brand = request.Brand ?? "",
                Category = request.Category ?? "",
                request.Price,
                request.DiscountPrice,
                Color = request.Color ?? "",
                request.StockQuantity
            });

        return Ok(new { message = "Товар добавлен", productId = id });
    }

    [HttpDelete("products/{id}")]
    public IActionResult DeleteProduct(int id)
    {
        using var connection = new SqliteConnection(DbConfig.ConnectionString);
        connection.Execute("UPDATE products SET is_active = 0 WHERE id = @Id", new { Id = id });
        return Ok(new { message = "Товар удален" });
    }
}

public class ProductRequest
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? Color { get; set; }
    public int StockQuantity { get; set; }
}