CREATE TABLE IF NOT EXISTS public.outbox_events
(
    id uuid NOT NULL,
    aggregate_type text COLLATE pg_catalog."default",
    aggregate_id uuid NOT NULL,
    type text COLLATE pg_catalog."default",
    payload text COLLATE pg_catalog."default",
    CONSTRAINT pk_outbox_events PRIMARY KEY(id)
);

ALTER TABLE IF EXISTS public.outbox_events OWNER to admin;

CREATE TABLE IF NOT EXISTS public.users
(
    id uuid NOT NULL,
    name character varying COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY(id)
);

ALTER TABLE IF EXISTS public.users OWNER to admin;