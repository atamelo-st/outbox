using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace DaprListener.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        
        [Topic("kafka-pubsub", "user_events")]
        [HttpPost("/notify")]
        public object Notify(object userEvent)
        {
            this._logger.LogInformation("{userEvent}", userEvent);
            return Ok(userEvent);
        }
    }
}