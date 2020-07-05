create table public.chat_zustand (
    id serial not null constraint chat_zustand_pk primary key,
    chat_id bigint not null,
    zustand integer default 0 not null
);

alter table
    public.chat_zustand owner to med_reminder_app;

create unique index chat_zustand_chat_id_uindex on chat_zustand (chat_id);

create table public.benutzer (
    id serial not null constraint user_pk primary key,
    name varchar not null,
    telegram_chat_id bigint not null constraint benutzer_chat_zustand_chat_id_fk references public.chat_zustand (chat_id)
);

alter table
    public.benutzer owner to med_reminder_app;

create unique index user_telegram_chat_id_uindex on public.benutzer (telegram_chat_id);

create table public.erinnerung (
    id serial not null constraint erinnerung_pk primary key,
    benutzer_id integer not null constraint erinnerung_benutzer_id_fk references public.benutzer,
    uhrzeit_utc timestamp not null,
    gueltig_ab_datim timestamp not null,
    zusaetzliche_erinnerung timestamp
);

alter table
    public.erinnerung owner to med_reminder_app;

create table public.erinnerung_gesendet (
    id serial not null constraint erinnerung_gesendet_pk primary key,
    erinnerung_id integer not null constraint erinnerung_gesendet_erinnerung_id_fk references public.erinnerung on update cascade on delete cascade,
    gesendet_um timestamp default CURRENT_TIMESTAMP not null,
    ist_zusaetzliche_erinnerung boolean default false not null
);

alter table
    public.erinnerung_gesendet owner to med_reminder_app;