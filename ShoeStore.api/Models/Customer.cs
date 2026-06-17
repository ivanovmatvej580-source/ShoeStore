namespace ShoeStoreAPI.Models;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string? BirthDate { get; set; }
    public string? Gender { get; set; }
    public double? ShoeSize { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}