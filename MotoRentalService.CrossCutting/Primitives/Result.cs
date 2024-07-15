
namespace MotoRentalService.CrossCutting.Primitives
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }

        protected Result(bool isSuccess, string? errorMessage, params object?[] args)
        {
            IsSuccess = isSuccess;
            ErrorMessage = string.Format(errorMessage ?? string.Empty, args);
        }

        public static Result Success() => new Result(true, null);

        public static Result Failure(string errorMessage, params object?[] args) => new Result(false, errorMessage, args);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        protected Result(bool isSuccess, T value, string? errorMessage, params object?[] args)
            : base(isSuccess, errorMessage, args)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);

        public static new Result<T> Failure(string errorMessage, params object?[] args) => new Result<T>(false, default!, errorMessage, args);
    }
}
