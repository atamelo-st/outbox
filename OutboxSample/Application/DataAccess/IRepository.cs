﻿namespace OutboxSample.Application.DataAccess;

public interface IRepository
{
}

public abstract record QueryResult
{
    public static Success<TData> OfSuccess<TData>(TData data, DataStore.ItemMetadata metadata) => new(data, metadata);

    public static Success<Common.Void> OfSuccess(DataStore.ItemMetadata metadata) => new(Common.Void.Instance, metadata);

    public static Success<Common.Void> OfSuccess() => OfSuccess(DataStore.ItemMetadata.Empty);

    public sealed record Success<TData>(TData Data, DataStore.ItemMetadata Metadata) : QueryResult<TData>, Success;

    public interface Success
    { 
        DataStore.ItemMetadata Metadata { get; }
    }

    public interface Failure
    {
        string Message { get; }

        public interface AlreadyExists : Failure { }
        public interface NotFound : Failure { }
        public interface ConcurrencyConflict : Failure { }
    }
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
        public sealed record AlreadyExists(string Message) : Failure(Message, ErrorCode.AlreadyExists), QueryResult.Failure.AlreadyExists;

        public sealed record NotFound(string Message) : Failure(Message, ErrorCode.NotFound), QueryResult.Failure.NotFound;

        public sealed record ConcurrencyConflict(string Message) : Failure(Message, ErrorCode.ConcurrencyConflict), QueryResult.Failure.ConcurrencyConflict;

        protected Failure(string message, ErrorCode errorCode) : this(new Description(message, errorCode))
        { }

        public string Message => this.WhatHappened.Text;

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

    public readonly record struct ItemMetadata
    {
        private readonly uint version;
        private readonly DateTime createdAt;
        private readonly DateTime updatedAt;

        private readonly bool isEmpty = false;

        public static readonly ItemMetadata Empty = new();

        public bool IsEmpty => this.isEmpty;

        public uint Version => this.IsEmpty ? throw EmptyMetadata() : this.version;

        public DateTime CreatedAt => this.IsEmpty ? throw EmptyMetadata() : this.createdAt;

        public DateTime UpdatedAt => this.IsEmpty ? throw EmptyMetadata() : this.updatedAt;

        public ItemMetadata() : this(default) => this.isEmpty = true;

        public ItemMetadata(uint version, DateTime updatedAt = default, DateTime createdAt = default)
        {
            this.version = version;
            this.updatedAt = updatedAt;
            this.createdAt = createdAt;
        }

        private static Exception EmptyMetadata() => new InvalidOperationException("Can't read empty metadata.");
    }
}