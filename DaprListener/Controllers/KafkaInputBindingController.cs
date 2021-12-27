using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

namespace DaprListener.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KafkaInputBindingController : ControllerBase
    {
        private readonly ILogger<KafkaInputBindingController> _logger;

        public KafkaInputBindingController(ILogger<KafkaInputBindingController> logger)
        {
            _logger = logger;
        }
        
        [HttpPost("/user-added")]
        // TODO: test if/how Kafka headers are propagated
        public async Task<IActionResult> Notify(JsonElement userEventJson)
        {
            // TODO: move it to a separate model binder or find out if there is a way to configure it
            string jsonString = userEventJson.GetString()!;
            var userAddedEvent = JsonConvert.DeserializeObject<UserAddedEvent>(jsonString)!;

            this._logger.LogInformation("Processing event for user {userId}...", userAddedEvent.UserId);
            await Task.Delay(2000);
            this._logger.LogInformation("Processing complete {userId}.", userAddedEvent.UserId);
            this._logger.LogInformation("                                                         ");

            // this._logger.LogInformation("Input binding: {userEvent}", userAddedEvent);
            
            return Ok();
        }

        public readonly record struct UserAddedEvent
        {
            public Guid UserId { get; }
            public string UserName { get; }

            public UserAddedEvent(Guid userId, string userName)
            {
                ArgumentNullException.ThrowIfNull(userName, nameof(userName));

                UserId = userId;
                UserName = userName;
            }

            //public UserAddedEvent()
            //{
            //    throw new NotSupportedException("Must use constructor with parameters.");
            //}
        }

    }
}