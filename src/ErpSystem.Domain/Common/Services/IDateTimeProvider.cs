namespace ErpSystem.Domain.Common.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}
