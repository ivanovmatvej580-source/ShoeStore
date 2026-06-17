namespace ShoeStoreAPI.Models;

public record LoginRequest(string Login);
public record RegisterRequest(
    string FirstName, string LastName, string Phone,
    string? Email, string? BirthDate, string? Gender, double? ShoeSize);
public record CheckoutRequest(int CustomerId, decimal TotalAmount);
public record FilterRequest(string? Search, string? Category, string? Brand, string? Sort);

public record ProductDto(int Id, string Name, string Brand, string Category, decimal CurrentPrice, string? Color, int StockQuantity);
public record OrderDto(int Id, string OrderDate, string Status, decimal TotalAmount, string? CustomerName);
public record CustomerDto(int Id, string FirstName, string LastName, string Phone, string? Email, string? BirthDate, string? Gender, double? ShoeSize);