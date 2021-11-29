using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;
using OutboxSample.Model;

namespace OutboxSample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
             "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWorkFactory unitOfWorkFactory;
    private readonly ILogger<WeatherForecastController> logger;


    public WeatherForecastController(
        IUserRepository userRepository,
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<WeatherForecastController> logger)
    {
        this.userRepository = userRepository;
        this.unitOfWorkFactory = unitOfWorkFactory;
        this.logger = logger;
    }

    [HttpGet]
    public IEnumerable<User> Get()
    {
        IEnumerable<User> users = this.userRepository.GetAll();

        return users;

        // NOTE: this is just to test IUserRepository.GetAll() from a UnitOfWork
        // a stupid use-case, but it should work!

        //using (IUnitOfWork work = this.unitOfWorkFactory.Begin())
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

        using (IUnitOfWork work = this.unitOfWorkFactory.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            repo.Add(new(Guid.NewGuid(), DateTime.Now.ToString()));

            //IOutbox outbox = work.GetOutbox();

            //outbox.Publish((EventEnvelope<UserAddedEvent>)null!);

            saved = work.Commit();
        }

        return Ok(saved ? "Saved" : "Not saved");
    }
}
