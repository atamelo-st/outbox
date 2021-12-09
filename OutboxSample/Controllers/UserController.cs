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
    public IEnumerable<User> Get()
    {
        IEnumerable<User> users = this.userRepository.GetAll();

        return users;
    }


    [HttpPost]
    public IActionResult AddUser([FromServices] IUnitOfWorkFactory unitOfWork)
    {
        bool saved;

        using (IUnitOfWork work = unitOfWork.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            User newUser = new(SequentialUuid.New(), DateTime.Now.ToString());

            repo.Add(newUser);

            IOutbox outbox = work.GetOutbox();

            var userAddedEvent = new UserAddedEvent(SequentialUuid.New(), newUser.Id, newUser.Name);

            // TODO: get agg version from the repo.Add response
            EventEnvelope<UserAddedEvent> envelope = this.WrapEvent(userAddedEvent, rootApplicationAggregateId, 0);

            outbox.Send(envelope);

            saved = work.Commit();
        }

        return Ok(saved ? "Saved" : "Not saved");
    }


    [HttpPost("v2")]
    public IActionResult PostV2([FromServices] IUnitOfWorkFactory unitOfWork)
    {
        bool saved;

        using (IUnitOfWork work = unitOfWork.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            var users = new User[]
            {
                new(Guid.NewGuid(), DateTime.Now.ToString()),
                new(Guid.NewGuid(), DateTime.Now.ToString())
            };

            bool added = repo.AddMany(users);

            if (added is not true)
            {
                return Ok("Nothing added");
            }

            IOutbox outbox = work.GetOutbox();

            // outbox.SendMany(users.Select(user => new UserAddedEvent(user.Id, user.Name)).ToArray());

            saved = work.Commit();
        }

        return Ok(saved ? "Saved" : "Not saved");
    }
}
