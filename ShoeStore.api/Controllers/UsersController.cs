using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ShoeStore.api.Data;
using System.Linq;

namespace ShoeStore.api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        using var connection = new SqliteConnection(DbConfig.ConnectionString);
        var users = connection.Query(@"
            SELECT 
                id AS Id,
                login AS Login,
                role AS Role,
                first_name AS FirstName,
                last_name AS LastName,
                phone AS Phone,
                email AS Email
            FROM customers
            ORDER BY id");
        return Ok(users);
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Login))
            return BadRequest(new { message = "Логин обязателен" });

        using var connection = new SqliteConnection(DbConfig.ConnectionString);
        var exists = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM customers WHERE login = @Login", new { Login = request.Login });
        if (exists > 0)
            return BadRequest(new { message = "Логин занят" });

        connection.Execute(@"
            INSERT INTO customers (login, password, role, first_name, last_name, phone, email) 
            VALUES (@Login, @Password, 'user', @FirstName, @LastName, @Phone, @Email)",
            new
            {
                Login = request.Login.Trim(),
                Password = request.Password ?? "123",
                FirstName = string.IsNullOrWhiteSpace(request.FirstName) ? request.Login : request.FirstName,
                LastName = string.IsNullOrWhiteSpace(request.LastName) ? "" : request.LastName,
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? "" : request.Phone,
                Email = string.IsNullOrWhiteSpace(request.Email) ? "" : request.Email
            });

        return Ok(new { message = "Регистрация успешна!", login = request.Login });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Login))
            return Unauthorized(new { message = "Неверный логин или пароль" });

        using var connection = new SqliteConnection(DbConfig.ConnectionString);
        var user = connection.QueryFirstOrDefault(@"
            SELECT 
                id AS userId,
                login AS login,
                role AS role
            FROM customers 
            WHERE login = @Login AND password = @Password",
            new { Login = request.Login, Password = request.Password });

        if (user == null)
            return Unauthorized(new { message = "Неверный логин или пароль" });

        return Ok(user);
    }
}

public class RegisterRequest
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class LoginRequest
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}