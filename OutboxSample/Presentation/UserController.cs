using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Eventing;
using OutboxSample.Common;
using OutboxSample.DomainModel;
using OutboxSample.DomainModel.Events;
using static OutboxSample.Application.DataAccess.QueryResult;

namespace OutboxSample.Presentation;

[ApiController]
[Route("[controller]")]
public class UserController : ApplicationControllerBase
{
    // TODO: remove; it's just a temporary stand-in
    private static readonly Guid rootApplicationAggregateId = Guid.Parse("ef1e8203-209c-4c83-9284-2da9e17ddd6e");

    private readonly IUserRepository userRepository;
    private readonly ILogger<UserController> logger;

    public UserController(
        IUserRepository userRepository,
        IEventMetadataProvider eventMetadataProvider,
        ITimeProvider timeProvider,
        ILogger<UserController> logger) : base(eventMetadataProvider, timeProvider)
    {
        ArgumentNullException.ThrowIfNull(userRepository, nameof(userRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.userRepository = userRepository;
        this.logger = logger;
    }

    [HttpGet]
    // TODO: return smth like GetUsersResponse.
    public async Task<IActionResult> Get()
    {
        QueryResult<IEnumerable<DataStore.Item<User>>> getAllQueryResult = await userRepository.GetAllAsync();

        IActionResult actionResult = getAllQueryResult switch
        {
            // TODO: convert data + matadata into a response
            Success<IEnumerable<DataStore.Item<User>>> success => Ok(success.Data),

            _ => UnknownFailure(),
        };

        return actionResult;
    }


    [HttpPost]
    // TODO: introduce AddUserCommand
    // TODO: separate presentation logic and application logic - introduce smth like User-Command-Query-Handler (might be UseService for starters..)
    public async Task<IActionResult> AddUser([FromServices] IUnitOfWorkFactory unitOfWork)
    {
        await using IUnitOfWork work = await unitOfWork.BeginAsync();
        var repo = work.GetRepository<IUserRepository>();

        User newUser = new(SequentialUuid.New(), DateTime.Now.ToString());

        uint startingVersion = 0;
        QueryResult<int> addQueryResult = await repo.AddAsync(newUser, createdAt: TimeProvider.UtcNow, startingVersion);

        if (addQueryResult is Failure)
        {
            return addQueryResult switch
            {
                QueryResult<int>.Failure.AlreadyExists failure => base.Conflict($"User with Id=[{newUser.Id}] already exists."),

                QueryResult<int>.Failure.ConcurrencyConflict failure => base.Conflict(failure.Message),

                _ => UnknownFailure(),
            };
        }

        IOutbox outbox = work.GetOutbox();

        var userAddedEvent = new UserAddedEvent(SequentialUuid.New(), newUser.Id, newUser.Name);

        EventEnvelope envelope = WrapEvent(userAddedEvent, rootApplicationAggregateId, aggregateVersion: startingVersion);

        await outbox.SendAsync(envelope);

        bool saved = await work.CommitAsync();

        return Ok(saved ? "User added." : "Not added.");
    }


    [HttpPost("v2")]
    public async Task<IActionResult> PostV2([FromServices] IUnitOfWorkFactory unitOfWork)
    {
        await using IUnitOfWork work = await unitOfWork.BeginAsync();
        var repo = work.GetRepository<IUserRepository>();

        var users = new User[]
        {
                new(Guid.NewGuid(), DateTime.Now.ToString()),
                new(Guid.NewGuid(), DateTime.Now.ToString())
        };

        QueryResult<int> queryResult = await repo.AddManyAsync(users, createdAt: TimeProvider.UtcNow, startingVersion: 0);

        if (queryResult is not Success<int> recordsSaved)
        {
            return Ok("Nothing added");
        }

        IOutbox outbox = work.GetOutbox();

        // outbox.SendMany(users.Select(user => new UserAddedEvent(user.Id, user.Name)).ToArray());

        bool saved = await work.CommitAsync();

        return Ok(saved ? $"{recordsSaved.Data} records saved." : "Not saved.");
    }

    private IActionResult UnknownFailure() => base.StatusCode(500, "Something unexpected happened.");
}
