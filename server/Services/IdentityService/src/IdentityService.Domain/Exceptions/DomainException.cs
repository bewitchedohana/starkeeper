using System;

namespace IdentityService.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string error) : base(error) { }
    public DomainException(string error, Exception innerException) : base(error, innerException) {}
    public static void ThrowWhen(bool condition, string message)
    {
        if(condition)
        {
            throw new DomainException(message);
        }
    }
}
