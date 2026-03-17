namespace ErpSystem.SharedKernel.Results;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
    public static readonly Error NotFound = new("Error.NotFound", "The requested resource was not found.");
    public static readonly Error Validation = new("Error.Validation", "A validation error occurred.");
    public static readonly Error Conflict = new("Error.Conflict", "A conflict occurred.");
    public static readonly Error Unauthorized = new("Error.Unauthorized", "Unauthorized access.");
    public static readonly Error Forbidden = new("Error.Forbidden", "Access is forbidden.");

    public static Error Custom(string code, string message) => new(code, message);
}

public sealed record ValidationError(string Code, string Message, string PropertyName) : Error(Code, Message)
{
    public static ValidationError FromProperty(string propertyName, string message) =>
        new($"Validation.{propertyName}", message, propertyName);
}
