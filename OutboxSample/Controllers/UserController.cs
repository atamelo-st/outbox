using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Model;

namespace OutboxSample.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ApplicationControllerBase
{
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

            repo.Add(new(Guid.NewGuid(), DateTime.Now.ToString()));

            //IOutbox outbox = work.GetOutbox();

            //outbox.Publish((EventEnvelope<UserAddedEvent>)null!);

            saved = work.Commit();
        }

        return Ok(saved ? "Saved" : "Not saved");
    }

    [HttpPost("v2")]
    public IActionResult PostV2()
    {
        bool saved;

        using (IUnitOfWork work = UnitOfWork.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            repo.AddMany(new User[]
            {
                new(Guid.NewGuid(), DateTime.Now.ToString()),
                new(Guid.NewGuid(), DateTime.Now.ToString())
            });

            //IOutbox outbox = work.GetOutbox();

            //outbox.Publish((EventEnvelope<UserAddedEvent>)null!);

            saved = work.Commit();
        }

        return Ok(saved ? "Saved" : "Not saved");
    }
}
