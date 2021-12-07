# outbox
1. `docker-compose up`
2. To create the debezium connector run this script https://github.com/atamelo-st/outbox/blob/master/scripts/debezium/init-debezium.sh
3. Install [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/)
4. Run `dapr run --app-id dapr-listener --app-port 5076 --dapr-http-port 3602 --dapr-grpc-port 60002 --components-path ../components -- dotnet run --urls="http://0.0.0.0:5076"` to run Dapr consumer.
