namespace MyShop.Server.GraphQL.DTOs;

public class CreateUserInput
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserInput
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Photo { get; set; }
    public string? Role { get; set; }
}
