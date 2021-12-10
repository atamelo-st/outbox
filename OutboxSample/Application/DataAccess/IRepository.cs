namespace OutboxSample.Application.DataAccess;

public interface IRepository
{
}

public abstract record QueryResult
{
    public static Success<TData> OfSuccess<TData>(TData data, DataStore.ItemMetadata metadata) => new(data, metadata);

    public sealed record Success<TData>(TData Data, DataStore.ItemMetadata Metadata) : QueryResult<TData>, Success;

    public interface Success { }

    public interface Failure { }
}

public abstract record QueryResult<TExpectedData> : QueryResult
{
    public static class OfFailure
    {
        public static Failure AlreadyExists(string message = "Already exists.") => new Failure.AlreadyExists(message);

        public static Failure NotFound(string message = "Not found.") => new Failure.NotFound(message);

        public static Failure ConcurrencyConflict(string message = "Concurrency conflict.") => new Failure.ConcurrencyConflict(message);
    }

    new public abstract record Failure(Failure.Description WhatHappened) : QueryResult<TExpectedData>, QueryResult.Failure
    {
        public sealed record AlreadyExists(string Message) : Failure(Message, ErrorCode.AlreadyExists);

        public sealed record NotFound(string Message) : Failure(Message, ErrorCode.NotFound);

        public sealed record ConcurrencyConflict(string Message) : Failure(Message, ErrorCode.ConcurrencyConflict);

        protected Failure(string message, ErrorCode errorCode) : this(new Description(message, errorCode))
        { }

        public readonly record struct Description(string Text, ErrorCode ErrorCode);

        public enum ErrorCode
        {
            Undefined = 0,
            // Unexpected = 1,
            NotFound = 2,
            AlreadyExists = 3,
            ConcurrencyConflict = 4,
        }
    }
}

public static class DataStore
{
    public readonly record struct Item<TData>(TData Data, ItemMetadata Metadata);

    public readonly record struct ItemMetadata(DateTime CreateAt, DateTime UpatedAt, uint Version)
    {
        private readonly bool isEmpty = false;

        public static readonly ItemMetadata Empty = new();

        public bool IsEmpty => this.isEmpty;

        public ItemMetadata() : this(default, default, default) => this.isEmpty = true;
    }
}