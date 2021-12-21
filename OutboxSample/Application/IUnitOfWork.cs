﻿using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Eventing;

namespace OutboxSample.Application;

public interface IUnitOfWork : IDisposable
{
    void Commit();
    void Rollback();

    TRepository GetRepository<TRepository>() where TRepository : IRepository, ISupportUnitOfWork;

    IOutbox GetOutbox();

    public class PendingTransactionException : Exception
    {
        public PendingTransactionException(string message) : base(message)
        {
        }
    }
}

