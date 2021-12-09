﻿using OutboxSample.Model.Events;

namespace OutboxSample.Application;

public interface IOutbox : ISupportUnitOfWork
{
    bool Send<TEvent>(EventEnvelope<TEvent> envelope) where TEvent : IEvent;

    bool Send<TEvent>(IReadOnlyList<EventEnvelope<TEvent>> events) where TEvent : IEvent;
}
