using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Model;

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
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<UserController> logger) : base(unitOfWorkFactory)
    {
        this.userRepository = userRepository;
        this.logger = logger;
    }

    [HttpGet]
    public IEnumerable<User> Get()
    {
        IEnumerable<User> users = this.userRepository.GetAll();

        return users;

        // NOTE: this is just to test IUserRepository.GetAll() from a UnitOfWork
        // a stupid use-case, but it should work!

        //using (IUnitOfWork work = UnitOfWork.Begin())
        //{
        //    var repo = work.GetRepository<IUserRepository>();

        //    IEnumerable<User> users = repo.GetAll();

        //    return users;
        //}
    }


    [HttpPost]
    public IActionResult Post()
    {
        bool saved;

        using (IUnitOfWork work = UnitOfWork.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            User newUser = new(Guid.NewGuid(), DateTime.Now.ToString());

            repo.Add(newUser);

            IOutbox outbox = work.GetOutbox();

            // TODO: replace Guid.NewGuid()
            var userAddedEvent = new UserAddedEvent(Guid.NewGuid(), newUser.Id, newUser.Name);

            // TODO: get agg version from the repo.Add response
            EventEnvelope<UserAddedEvent> envelope = WrapEvent(userAddedEvent, rootApplicationAggregateId, 0);

            outbox.Send(envelope);

            saved = work.Commit();
        }

        return Ok(saved ? "Saved" : "Not saved");
    }

    private static EventEnvelope<TEvent> WrapEvent<TEvent>(
        TEvent @event,
        Guid aggregateId,
        uint aggregateVersion
    ) where TEvent : IEvent
    {
        // TODO: infer from event type
        string eventType = "";
        string aggregateType = "";
        uint eventSchemaVersion = 0;

        // TODO: replace with time getter abstraction
        DateTime timestamp = DateTime.Now; 

        return new EventEnvelope<TEvent>(@event, eventType, aggregateId, aggregateType, timestamp, aggregateVersion, eventSchemaVersion);
    }

    [HttpPost("v2")]
    public IActionResult PostV2()
    {
        bool saved;

        using (IUnitOfWork work = UnitOfWork.Begin())
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
