namespace OutboxSample.Application;

public interface IRepository
{
}


public abstract record QueryResult
{
    public static Success<TData> OfSuccess<TData>(TData payload) => new Success<TData>(payload);

    public record Success<TData>(TData Data) : QueryResult<TData>, Success;

    public interface Success { }
}

public abstract record QueryResult<TExpectedData> : QueryResult
{
    public static class OfFailure
    {
        public static Failure AlreadyExists(string? message = null) => new Failure.AlreadyExists(message);

        public static Failure NotFound(string? message = null) => new Failure.NotFound(message);

        public static Failure ConcurrencyConflict(string? message = null) => new Failure.ConcurrencyConflict(message);
    }

    public abstract record Failure(Failure.Description WhatHappened) : QueryResult<TExpectedData>
    {
        public record AlreadyExists(string? message = null) : Failure(message, ErrorCode.AlreadyExists);

        public record NotFound(string? message = null) : Failure(message, ErrorCode.NotFound);

        public record ConcurrencyConflict(string? message = null) : Failure(message, ErrorCode.ConcurrencyConflict);

        public Failure(string? message, ErrorCode errorCode) : this(new Description(message, errorCode))
        { }

        public readonly record struct Description(string? Text, ErrorCode errorCode);

        public enum ErrorCode
        {
            Undefined = 0,
            NotFound = 1,
            AlreadyExists = 2,
            ConcurrencyConflict = 3,
        }
    }
}

