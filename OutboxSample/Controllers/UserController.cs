using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Common;
using OutboxSample.Model;
using OutboxSample.Model.Events;

namespace OutboxSample.Controllers;

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
        this.userRepository = userRepository;
        this.logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        QueryResult<IEnumerable<User>> getAllQueryResult = this.userRepository.GetAll();

        IActionResult actionResult = getAllQueryResult switch
        {
            QueryResult.Success<IEnumerable<User>> success => Ok(success.Data),

            _ => this.UnknownFailure(),
        };

        return actionResult;
    }


    [HttpPost]
    public IActionResult AddUser([FromServices] IUnitOfWorkFactory unitOfWork)
    {
        using (IUnitOfWork work = unitOfWork.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            User newUser = new(SequentialUuid.New(), DateTime.Now.ToString());

            QueryResult<int> addQueryResult = repo.Add(newUser);

            if (addQueryResult is QueryResult.Failure)
            {
                return addQueryResult switch
                {
                    QueryResult<int>.Failure.AlreadyExists failure => base.Conflict($"User with Id=[{newUser.Id}] already exists."),

                    QueryResult<int>.Failure.ConcurrencyConflict failure => base.Conflict(failure.Message),

                    _ => this.UnknownFailure(),
                };
            }

            IOutbox outbox = work.GetOutbox();

            var userAddedEvent = new UserAddedEvent(SequentialUuid.New(), newUser.Id, newUser.Name);

            // TODO: get agg version from the repo.Add response
            EventEnvelope envelope = this.WrapEvent(userAddedEvent, rootApplicationAggregateId, 0);

            outbox.Send(envelope);

            bool saved = work.Commit();

            return Ok(saved ? "User added." : "Not added.");
        }
    }


    [HttpPost("v2")]
    public IActionResult PostV2([FromServices] IUnitOfWorkFactory unitOfWork)
    {
        using (IUnitOfWork work = unitOfWork.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            var users = new User[]
            {
                new(Guid.NewGuid(), DateTime.Now.ToString()),
                new(Guid.NewGuid(), DateTime.Now.ToString())
            };

            QueryResult<int> queryResult = repo.AddMany(users);

            if (queryResult is not QueryResult.Success<int> recordsSaved)
            {
                return Ok("Nothing added");
            }

            IOutbox outbox = work.GetOutbox();

            // outbox.SendMany(users.Select(user => new UserAddedEvent(user.Id, user.Name)).ToArray());

            bool saved = work.Commit();

            return Ok(saved ? $"{recordsSaved.Data} records saved." : "Not saved.");
        }
    }

    private IActionResult UnknownFailure() => base.StatusCode(500, "Something unexpected happened.");
}
