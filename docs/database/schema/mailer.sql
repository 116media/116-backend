CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'mailer') THEN
        CREATE SCHEMA mailer;
    END IF;
END $EF$;

CREATE TABLE mailer.newsletter_subscribers (
    id uuid NOT NULL,
    email character varying(320) NOT NULL,
    status character varying(30) NOT NULL,
    confirmation_token character varying(64) NOT NULL,
    unsubscribe_token character varying(64) NOT NULL,
    confirmed_at timestamp with time zone,
    unsubscribed_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_newsletter_subscribers PRIMARY KEY (id)
);

CREATE TABLE mailer.outbox_emails (
    id uuid NOT NULL,
    recipient_address character varying(320) NOT NULL,
    recipient_name character varying(200),
    subject character varying(500) NOT NULL,
    html_body text NOT NULL,
    text_body text NOT NULL,
    template character varying(100) NOT NULL,
    status character varying(20) NOT NULL,
    attempt_count integer NOT NULL,
    next_attempt_at timestamp with time zone NOT NULL,
    last_error character varying(1000),
    sent_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_outbox_emails PRIMARY KEY (id)
);

CREATE UNIQUE INDEX ix_newsletter_subscribers_confirmation_token ON mailer.newsletter_subscribers (confirmation_token);

CREATE UNIQUE INDEX ix_newsletter_subscribers_email ON mailer.newsletter_subscribers (email);

CREATE UNIQUE INDEX ix_newsletter_subscribers_unsubscribe_token ON mailer.newsletter_subscribers (unsubscribe_token);

CREATE INDEX ix_outbox_emails_status_next_attempt_at ON mailer.outbox_emails (status, next_attempt_at);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260816194237_AddMailerOutboxAndNewsletter', '9.0.4');

CREATE TABLE mailer.notifications (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    type character varying(50) NOT NULL,
    title character varying(200) NOT NULL,
    body character varying(500) NOT NULL,
    link_path character varying(300),
    read_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_notifications PRIMARY KEY (id)
);

CREATE INDEX ix_notifications_user_id_created_at ON mailer.notifications (user_id, created_at DESC);

CREATE INDEX ix_notifications_user_id_read_at ON mailer.notifications (user_id, read_at);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260817093610_AddNotifications', '9.0.4');

ALTER TABLE mailer.outbox_emails ADD lease_expires_at timestamp with time zone;

CREATE TABLE mailer.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer NOT NULL DEFAULT 0,
    last_error text,
    CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id)
);

CREATE TABLE mailer.processed_domain_events (
    event_id uuid NOT NULL,
    handler_name character varying(200) NOT NULL,
    processed_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_processed_domain_events PRIMARY KEY (event_id, handler_name)
);

CREATE INDEX ix_outbox_emails_lease_expires_at ON mailer.outbox_emails (lease_expires_at);

CREATE INDEX ix_domain_event_outbox_pending ON mailer.domain_event_outbox (occurred_on) WHERE dispatched_at IS NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260909134511_AddDomainEventOutboxAndProcessedEvents', '9.0.4');

DROP TABLE mailer.processed_domain_events;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260910172748_DropRedundantProcessedDomainEvents', '9.0.4');

COMMIT;

