namespace Company.Service.Application.Common.Interfaces.UserContext;

public record User(
    string Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    int TenantId)
{
    public string FullName => $"{FirstName} {LastName}";
}