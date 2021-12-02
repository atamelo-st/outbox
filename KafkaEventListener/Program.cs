using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        CancellationTokenSource cancellation = new();

        Console.CancelKeyPress += (s, e) => cancellation.Cancel();

        RunKafkaLoop(cancellation.Token);

        Console.WriteLine("Exiting..");
    }

    private static void RunKafkaLoop(CancellationToken cancellationSignal)
    {
        while (cancellationSignal.IsCancellationRequested is not true) ;

        KafkaConsumerConfig config = new();

        var builder =
            new ConsumerBuilder<string, UserAddedEvent>(config)
                .SetValueDeserializer(new KafkaDeserializer<UserAddedEvent>());

        using (IConsumer<string, UserAddedEvent> consumer = builder.Build())
        {
            consumer.Subscribe(config.Topic);

            while (cancellationSignal.IsCancellationRequested == false)
            {
                try
                {
                    ConsumeResult<string, UserAddedEvent> result = consumer.Consume(3000);

                    if (result != null)
                    {
                        Console.WriteLine($"Key: {result.Message.Key}\nPayload: {result.Message.Value}");

                        consumer.Commit(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }
            }
        }
    }

    public class KafkaConsumerConfig : ConsumerConfig
    {
        public string Topic { get; set; }
        public KafkaConsumerConfig()
        {
            AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            EnableAutoOffsetStore = false;
            Topic = "user_events";
            GroupId = "user_events_notification_group";
            BootstrapServers = "localhost:9092";
        }
    }


    internal sealed class KafkaDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var dataJsonString = Encoding.UTF8.GetString(data);

            // deserializing twice because of double serialization of event payload.
            var normalizedJsonString = JsonConvert.DeserializeObject<string>(dataJsonString)!;

            return JsonConvert.DeserializeObject<T>(normalizedJsonString)!;
        }
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


