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

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly ILogger<WeatherForecastController> _logger;


    public WeatherForecastController(
        IUnitOfWorkFactory unitOfWorkFactory,
        ILogger<WeatherForecastController> logger)
    {
        this._unitOfWorkFactory = unitOfWorkFactory;
        this._logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }


    [HttpPost]
    public IActionResult Post()
    {
        using (IUnitOfWork work = this._unitOfWorkFactory.Begin())
        {
            var repo = work.GetRepository<IUserRepository>();

            repo.Add(null!);

            IOutbox outbox = work.GetOutbox();

            outbox.Publish((EventEnvelope<UserAddedEvent>)null!);

            work.Commit();
        }

        return Ok();
    }
}
