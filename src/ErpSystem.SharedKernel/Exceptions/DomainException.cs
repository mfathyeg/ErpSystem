namespace ErpSystem.SharedKernel.Exceptions;

public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base("Entity.NotFound", $"{entityName} with id '{id}' was not found.")
    {
    }
}

public class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string code, string message)
        : base(code, message)
    {
    }
}

public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string entityName, object id)
        : base("Concurrency.Conflict", $"Concurrency conflict detected for {entityName} with id '{id}'.")
    {
    }
}

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation.Failed", "One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
