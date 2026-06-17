using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ShoeStore.api.Data;

namespace ShoeStore.api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        using var connection = new SqliteConnection(DbConfig.ConnectionString);
        var products = connection.Query(@"
            SELECT 
                id AS Id,
                name AS Name,
                brand AS Brand,
                category AS Category,
                CASE WHEN discount_price IS NOT NULL THEN discount_price ELSE price END AS CurrentPrice,
                color AS Color,
                stock_quantity AS StockQuantity
            FROM products 
            WHERE is_active = 1
            ORDER BY id");

        return Ok(products);
    }
}