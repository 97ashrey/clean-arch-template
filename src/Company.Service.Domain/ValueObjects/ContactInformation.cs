using Company.Service.Domain.Common.Types;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Common.Types.Utils;

public record ContactInformation()
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;

    private ContactInformation(string firstName, string lastName, string email, string phoneNumber) : this()
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static Result<ContactInformation, ValidationError> CreateNew(
        string firstName, string lastName, string email, string phoneNumber)
    {
        return Validate.ExecuteRules(
            Validate.NotEmpty(firstName, nameof(firstName)),
            Validate.NotEmpty(lastName, nameof(lastName)),
            Validate.NotEmpty(email, nameof(email)),
            Validate.NotEmpty(phoneNumber, nameof(phoneNumber))
        ).MapToValueResult(new ContactInformation(firstName, lastName, email, phoneNumber));
    }
}