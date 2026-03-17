using ErpSystem.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace ErpSystem.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            return CreateValidationResult<TResponse>(failures);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(
        List<FluentValidation.Results.ValidationFailure> failures)
        where TResult : Result
    {
        var firstError = failures.First();
        var error = Error.Custom(
            $"Validation.{firstError.PropertyName}",
            firstError.ErrorMessage);

        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(error) as TResult)!;
        }

        var resultType = typeof(TResult).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
            .MakeGenericMethod(resultType);

        return (TResult)failureMethod.Invoke(null, new object[] { error })!;
    }
}
