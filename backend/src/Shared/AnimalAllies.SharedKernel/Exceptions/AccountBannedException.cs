namespace AnimalAllies.SharedKernel.Exceptions;

public class AccountBannedException(string? message) : Exception(message)
{
    public string? Message { get; } = message;
}