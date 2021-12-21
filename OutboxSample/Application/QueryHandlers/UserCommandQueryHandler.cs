﻿using OutboxSample.Application.Commands;
using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Eventing;
using OutboxSample.Application.Queries;
using OutboxSample.Common;
using OutboxSample.DomainModel;
using OutboxSample.DomainModel.Events;

namespace OutboxSample.Application.QueryHandlers;

public class UserCommandQueryHandler :
    IQueryHandler<GetUserQuery, QueryResult<User>>,
    IQueryHandler<GetAllUsersQuery, QueryResult<IEnumerable<DataStore.Item<User>>>>,
    ICommandHandler<AddUserCommand, AddUserCommandResult>
{
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWorkFactory unitOfWork;
    private readonly ITimeProvider timeProvider;
    private readonly IEventMetadataProvider eventMetadataProvider;

    // TODO: remove; it's just a temporary stand-in
    private static readonly Guid rootApplicationAggregateId = Guid.Parse("ef1e8203-209c-4c83-9284-2da9e17ddd6e");

    public UserCommandQueryHandler(
        IUserRepository userRepository,
        IUnitOfWorkFactory unitOfWork,
        ITimeProvider timeProvider,
        IEventMetadataProvider eventMetadataProvider
    )
    {
        ArgumentNullException.ThrowIfNull(userRepository, nameof(userRepository));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        ArgumentNullException.ThrowIfNull(eventMetadataProvider, nameof(eventMetadataProvider));

        this.userRepository = userRepository;
        this.unitOfWork = unitOfWork;
        this.timeProvider = timeProvider;
        this.eventMetadataProvider = eventMetadataProvider;
    }

    public async Task<QueryResult<User>> HandleAsync(GetUserQuery query)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        QueryResult<User> queryResult = await this.userRepository.GetAsync(query.userId);

        return queryResult;
    }

    public async Task<QueryResult<IEnumerable<DataStore.Item<User>>>> HandleAsync(GetAllUsersQuery query)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        QueryResult<IEnumerable<DataStore.Item<User>>> queryResult = await this.userRepository.GetAllAsync();

        return queryResult;
    }

    public async Task<AddUserCommandResult> HandleAsync(AddUserCommand command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        await using (IUnitOfWork work = await this.unitOfWork.BeginAsync("add-user"))
        {
            var repo = work.GetRepository<IUserRepository>();

            User newUser = new(command.UserId, command.UserName);

            uint startingVersion = 0;
            QueryResult addUserResult = await repo.AddAsync(newUser, createdAt: this.timeProvider.UtcNow, startingVersion);

            if (addUserResult is QueryResult.Success)
            {
                IOutbox outbox = work.GetOutbox();

                var userAddedEvent = new UserAddedEvent(SequentialUuid.New(), newUser.Id, newUser.Name);

                EventEnvelope envelope = this.WrapEvent(userAddedEvent, rootApplicationAggregateId, aggregateVersion: startingVersion);

                await outbox.SendAsync(envelope);

                await work.CommitAsync();
            }

            return new AddUserCommandResult(addUserResult, startingVersion);
        }
    }

    private EventEnvelope WrapEvent<TEvent>(
        TEvent @event,
        Guid aggregateId,
        uint aggregateVersion
    ) where TEvent : IEvent
    {
        EventMetadata eventMetadata = this.eventMetadataProvider.GetMetadataFor(@event);

        DateTime timestamp = this.timeProvider.UtcNow;

        return new EventEnvelope(
            // NOTE: if TEvent is a struct, this is where boxing will happen
            @event,
            eventMetadata.EventType,
            aggregateId,
            eventMetadata.AgregateType,
            timestamp,
            aggregateVersion,
            eventMetadata.EventSchemaVersion
        );
    }
}
