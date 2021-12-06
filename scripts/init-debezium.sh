#!/bin/bash

curl -H 'Content-Type: application/json' kafka-connect-with-debezium-outbox:8083/connectors --data '
{
  "name": "outbox-connector",  
  "config": {
    "connector.class": "io.debezium.connector.postgresql.PostgresConnector", 
    "plugin.name": "pgoutput",
    "database.hostname": "postgres-outbox", 
    "database.port": "5432", 
    "database.user": "admin", 
    "database.password": "admin", 
    "database.dbname" : "testdatabase", 
    "database.server.name": "postgres-outbox", 
    "table.include.list": "public.outbox_events" 
  }
}'