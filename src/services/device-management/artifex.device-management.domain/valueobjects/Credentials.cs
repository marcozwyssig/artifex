using Artifex.Shared.Domain;

namespace Artifex.DeviceManagement.Domain.ValueObjects;

/// <summary>
/// Device credentials value object
/// </summary>
public class Credentials : ValueObject
{
    public string Username { get; private set; }
    public string Password { get; private set; }
    public string? EnablePassword { get; private set; }

    private Credentials(string username, string password, string? enablePassword = null)
    {
        Username = username;
        Password = password;
        EnablePassword = enablePassword;
    }

    public static Result<Credentials> Create(string username, string password, string? enablePassword = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result.Failure<Credentials>("Username cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure<Credentials>("Password cannot be empty");
        }

        if (username.Length < 3)
        {
            return Result.Failure<Credentials>("Username must be at least 3 characters long");
        }

        return Result.Success(new Credentials(username, password, enablePassword));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Username;
        yield return Password;
        yield return EnablePassword;
    }

    public Credentials UpdatePassword(string newPassword)
    {
        return new Credentials(Username, newPassword, EnablePassword);
    }

    public Credentials UpdateEnablePassword(string? newEnablePassword)
    {
        return new Credentials(Username, Password, newEnablePassword);
    }
}
