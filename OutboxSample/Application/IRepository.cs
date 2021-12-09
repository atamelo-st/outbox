namespace OutboxSample.Application;

public interface IRepository
{
}

public abstract record QueryResult<TPayload> : QueryResult;

public abstract record QueryResult
{
    public static Success<TPayload> OfSuccess<TPayload>(TPayload payload) => new Success<TPayload>(payload);

    public static class OfFailure
    {
        public static Failure AlreadyExists(string? message = null) => new Failure.AlreadyExists(message);

        public static Failure NotFound(string? message = null) => new Failure.NotFound(message);

        public static Failure ConcurrencyConflict(string? message = null) => new Failure.ConcurrencyConflict(message);
    }

    public record Success<TPayload> : QueryResult<TPayload>
    {
        public TPayload Payload { get; }

        internal Success(TPayload payload) => this.Payload = payload;
    }

    public abstract record Failure : QueryResult
    {
        public record AlreadyExists(string? message = null) : Failure(message, ErrorCode.AlreadyExists);

        public record NotFound(string? message = null) : Failure(message, ErrorCode.NotFound);

        public record ConcurrencyConflict(string? message = null) : Failure(message, ErrorCode.ConcurrencyConflict);

        public Description WhatHappened { get; }

        internal Failure(Description description) => this.WhatHappened = description;

        internal Failure(string? message, ErrorCode errorCode) : this(new Description(message, errorCode))
        {}

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

