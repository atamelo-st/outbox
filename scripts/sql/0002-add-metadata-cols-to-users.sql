ALTER TABLE public.users 
    ADD COLUMN IF NOT EXISTS created_at timestamp DEFAULT (now() at time zone 'utc') NOT NULL;

ALTER TABLE public.users 
    ADD COLUMN IF NOT EXISTS updated_at timestamp DEFAULT (now() at time zone 'utc') NOT NULL;

ALTER TABLE public.users 
    ADD COLUMN IF NOT EXISTS version integer NOT NULL DEFAULT 0;
