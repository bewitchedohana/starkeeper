namespace IdentityService.Domain.Errors;

public static class AccountErrors
{
    public static readonly Error InvalidEmailAddress = new(Convert.ToString(100), "Invalid email address");

    public static readonly Error WeakPassword = new(Convert.ToString(101), "Weak password");
}
