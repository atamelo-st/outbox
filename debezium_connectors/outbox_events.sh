#!/bin/bash

## configure debezium connector for the outbox table

create_connector () {
    curl -X POST http://localhost:8083/connectors -d @outbox_events_config.json \
        --header "Content-Type: application/json"
}

delete_connector () {
    curl -X DELETE http://localhost:8083/connectors/outbox_events_connector  \
        --header "Content-Type: application/json"
}

stop_connector () {
    curl -X PUT http://localhost:8083/connectors/outbox_events_connector/pause  \
        --header "Content-Type: application/json"
}

start_connector () {
    curl -X PUT http://localhost:8083/connectors/outbox_events_connector/resume  \
        --header "Content-Type: application/json"
}

"$@"
read