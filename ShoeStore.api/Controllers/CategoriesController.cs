using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace ShoeStore.api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly string _connectionString = "Data Source=Shoe.db";

    [HttpGet]
    public IActionResult Get()
    {
        using var connection = new SqliteConnection(_connectionString);
        var categories = connection.Query<string>(
            "SELECT DISTINCT category FROM products WHERE is_active = 1 ORDER BY category");
        return Ok(categories);
    }
}