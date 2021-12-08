# outbox
1. `docker-compose up`
2. Install [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/)
3. Run `dapr run --app-id dapr-listener --app-port 5076 --dapr-http-port 3602 --dapr-grpc-port 60002 --components-path ../components -- dotnet run --urls="http://0.0.0.0:5076"` to run Dapr consumer.
