using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Application.Commands;
using OutboxSample.Application.DataAccess;
using OutboxSample.Application.Queries;
using OutboxSample.DomainModel;
using static OutboxSample.Application.DataAccess.QueryResult;

namespace OutboxSample.Presentation;

[ApiController]
[Route("[controller]")]
public class UserController : Controller
{
    private readonly ILogger<UserController> logger;

    public UserController(ILogger<UserController> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.logger = logger;
    }

    [HttpGet("{userId}")]
    // TODO: return smth like GetUserResponse!
    public async Task<IActionResult> Get(GetUserQuery query, [FromServices] IQueryHandler<GetUserQuery, QueryResult<User>> queryHandler)
    {
        QueryResult<User> queryResult = await queryHandler.HandleAsync(query);

        IActionResult actionResult = queryResult switch
        {
            // TODO: convert data + matadata into a response!
            Success<User> success => Ok(success.Data),

            _ => UnknownFailure(),
        };

        return actionResult;
    }


    [HttpGet]
    // TODO: return smth like GetUsersResponse!
    public async Task<IActionResult> GetAll([FromServices] IQueryHandler<GetAllUsersQuery, QueryResult<IEnumerable<DataStore.Item<User>>>> queryHandler)
    {
        QueryResult<IEnumerable<DataStore.Item<User>>> queryResult = await queryHandler.HandleAsync(GetAllUsersQuery.Instance);

        IActionResult actionResult = queryResult switch
        {
            // TODO: convert data + matadata into a response!
            Success<IEnumerable<DataStore.Item<User>>> success => Ok(success.Data),

            _ => UnknownFailure(),
        };

        return actionResult;
    }


    [HttpPost]
    public async Task<IActionResult> AddUser(AddUserCommand command, [FromServices] ICommandHandler<AddUserCommand, AddUserCommandResult> commandHandler)
    {
        AddUserCommandResult commandResult = await commandHandler.HandleAsync(command);

        if (commandResult.QueryResult is Success)
        {
            return Ok($"User added. Version: {commandResult.Version}");
        }

        return commandResult.QueryResult switch
        {
            Failure.AlreadyExists => base.Conflict($"User with Id=[{command.UserId}] already exists."),

            Failure.ConcurrencyConflict failure => base.Conflict(failure.Message),

            _ => UnknownFailure(),
        };
    }


    //[HttpPost("v2")]
    //public IActionResult PostV2([FromServices] IUnitOfWorkFactory unitOfWork)
    //{
    //    using (IUnitOfWork work = unitOfWork.Begin())
    //    {
    //        var repo = work.GetRepository<IUserRepository>();

    //        var users = new User[]
    //        {
    //            new(Guid.NewGuid(), DateTime.Now.ToString()),
    //            new(Guid.NewGuid(), DateTime.Now.ToString())
    //        };

    //        QueryResult<int> queryResult = repo.AddMany(users, createdAt: TimeProvider.UtcNow, startingVersion: 0);

    //        if (queryResult is not Success<int> recordsSaved)
    //        {
    //            return Ok("Nothing added");
    //        }

    //        IOutbox outbox = work.GetOutbox();

    //        // outbox.SendMany(users.Select(user => new UserAddedEvent(user.Id, user.Name)).ToArray());

    //        bool saved = work.Commit();

    //        return Ok(saved ? $"{recordsSaved.Data} records saved." : "Not saved.");
    //    }
    //}

    private IActionResult UnknownFailure() => base.StatusCode(500, "Something unexpected happened.");
}
