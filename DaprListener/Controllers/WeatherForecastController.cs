using Dapr;
using Microsoft.AspNetCore.Mvc;

// USE:
// dapr run --app-id dapr-listener --app-port 5076 --dapr-http-port 3602 --dapr-grpc-port 60002 --components-path ../components -- dotnet run

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
        
        [HttpPost("/user-added")]
        public object Notify(object userEvent)
        {
            this._logger.LogInformation("Input binding: {userEvent}", userEvent);
            
            
            return Ok(userEvent);
        }

        public /*readonly*/ record struct UserAddedEvent
        {
            public Guid UserId { get; set; }
            public string UserName { get; set; }

            //public UserAddedEvent(Guid userId, string userName)
            //{
            //    ArgumentNullException.ThrowIfNull(userName, nameof(userName));

            //    UserId = userId;
            //    UserName = userName;
            //}

            //public UserAddedEvent()
            //{
            //    throw new NotSupportedException("Must use constructor with parameters.");
            //}
        }

    }
}