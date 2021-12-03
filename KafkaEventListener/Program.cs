using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;

public class Program
{
    public async static Task Main(string[] args)
    {
        var host =
           new HostBuilder()
               .ConfigureServices((hostContext, services) =>
                   services.AddHostedService<KafkaConsumerService>()
                )
               .UseConsoleLifetime()
               .Build();

        await host.RunAsync();
    }

    class KafkaConsumerService : IHostedService
    {
        private readonly CancellationTokenSource cancellation;

        private Task? kafkaLoop;

        public KafkaConsumerService()
        {
            this.cancellation = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.kafkaLoop = Task.Run(() =>
            {
                RunKafkaLoop(cancellation.Token);
            });

            Console.WriteLine("Kafka listerner started.");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stopping service...");

            this.cancellation.Cancel();
            await kafkaLoop!;

            Console.WriteLine("Service stopped.");
        }

        private static void RunKafkaLoop(CancellationToken cancellationSignal)
        {
            KafkaConsumerConfig config = new();

            var builder =
                new ConsumerBuilder<string, UserAddedEvent>(config)
                    .SetValueDeserializer(new KafkaDeserializer<UserAddedEvent>());

            using (IConsumer<string, UserAddedEvent> consumer = builder.Build())
            {
                consumer.Subscribe(config.Topic);

                while (cancellationSignal.IsCancellationRequested is not true)
                {
                    try
                    {
                        ConsumeResult<string, UserAddedEvent> result = consumer.Consume(cancellationSignal);

                        if (result is not null)
                        {
                            Console.WriteLine($"Key: {result.Message.Key}\nPayload: {result.Message.Value}");

                            consumer.Commit(result);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Listening cancelled.");
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }
                }
            }
        }

        private class KafkaDeserializer<T> : IDeserializer<T>
        {
            public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
            {
                var dataJsonString = Encoding.UTF8.GetString(data);

                // deserializing twice because of double serialization of event payload.
                var normalizedJsonString = JsonConvert.DeserializeObject<string>(dataJsonString)!;

                return JsonConvert.DeserializeObject<T>(normalizedJsonString)!;
            }
        }

        private class KafkaConsumerConfig : ConsumerConfig
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


