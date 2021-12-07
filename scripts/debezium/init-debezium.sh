#!/bin/sh

sleep 15;
curl -H 'Content-Type: application/json' kafka-connect-with-debezium-outbox:8083/connectors --data '
{
  "name": "outbox-connector",  
  "config": {
    "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
    "tasks.max": "1",
    "database.hostname": "postgres-outbox",
    "database.port": "5432",
    "database.user": "admin",
    "database.password": "admin",
    "database.dbname": "testdatabase",
    "database.server.name": "postgres-outbox",
    "table.include.list": "public.outbox_events",
    "slot.name": "outbox_events_slot",
    "transforms": "outbox",
    "transforms.outbox.type": "io.debezium.transforms.outbox.EventRouter",
    "transforms.outbox.table.field.event.id": "id",
    "transforms.outbox.table.field.event.key": "aggregate_id",
    "transforms.outbox.table.field.event.type": "type",
    "transforms.outbox.table.field.event.payload.id": "aggregate_id",
    "transforms.outbox.table.fields.additional.placement": "type:header:eventType",
    "transforms.outbox.route.by.field": "aggregate_type",
    "transforms.outbox.route.topic.replacement": "user_events",
    "key.converter": "org.apache.kafka.connect.storage.StringConverter",
    "key.converter.schemas.enable": "false",
    "value.converter": "org.apache.kafka.connect.json.JsonConverter",
    "value.converter.schemas.enable": "false",
    "include.schema.changes": "false"
  }
}'