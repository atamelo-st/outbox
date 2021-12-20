ALTER TABLE public.outbox_events 
    ADD COLUMN IF NOT EXISTS timestamp timestamp DEFAULT (now() at time zone 'utc') NOT NULL;

ALTER TABLE public.outbox_events 
    ADD COLUMN IF NOT EXISTS aggregate_version integer DEFAULT 0 NOT NULL;

ALTER TABLE public.outbox_events 
    ADD COLUMN IF NOT EXISTS event_schema_version integer DEFAULT 0 NOT NULL;
