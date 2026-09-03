CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'core') THEN
        CREATE SCHEMA core;
    END IF;
END $EF$;

CREATE TABLE core.files (
    id uuid NOT NULL,
    file_name character varying(255) NOT NULL,
    original_file_name character varying(255) NOT NULL,
    mime_type character varying(100) NOT NULL,
    storage_url character varying(2048) NOT NULL,
    size_in_bytes bigint NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    deleted_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_files PRIMARY KEY (id)
);

CREATE UNIQUE INDEX ix_files_file_name ON core.files (file_name);

CREATE INDEX ix_files_is_deleted ON core.files (is_deleted);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260111141504_InitCoreSchema', '9.0.4');

ALTER TABLE core.files ADD storage_key character varying(100);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260618215519_AddStorageKeyToFileEntity', '9.0.4');

DROP INDEX core.ix_files_file_name;

CREATE UNIQUE INDEX ix_files_file_name ON core.files (file_name) WHERE is_deleted = false;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260624221433_MakeFileNameUniqueIndexPartial', '9.0.4');

ALTER TABLE core.files ADD dominant_color_hex character varying(7);

ALTER TABLE core.files ADD foreground_color_hex character varying(7);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260628161137_AddFileColors', '9.0.4');

CREATE SCHEMA IF NOT EXISTS quartz;

SET LOCAL search_path TO quartz;

CREATE TABLE qrtz_job_details
  (
    sched_name TEXT NOT NULL,
    job_name TEXT NOT NULL,
    job_group TEXT NOT NULL,
    description TEXT NULL,
    job_class_name TEXT NOT NULL,
    is_durable BOOL NOT NULL,
    is_nonconcurrent BOOL NOT NULL,
    is_update_data BOOL NOT NULL,
    requests_recovery BOOL NOT NULL,
    job_data BYTEA NULL,
    PRIMARY KEY (sched_name, job_name, job_group)
);

CREATE TABLE qrtz_triggers
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    job_name TEXT NOT NULL,
    job_group TEXT NOT NULL,
    description TEXT NULL,
    next_fire_time BIGINT NULL,
    prev_fire_time BIGINT NULL,
    priority INTEGER NULL,
    trigger_state TEXT NOT NULL,
    trigger_type TEXT NOT NULL,
    start_time BIGINT NOT NULL,
    end_time BIGINT NULL,
    calendar_name TEXT NULL,
    misfire_instr SMALLINT NULL,
    misfire_orig_fire_time BIGINT NULL,
    execution_group VARCHAR(200) NULL,
    preferred_node VARCHAR(200) NULL,
    preferred_node_auto BOOL NOT NULL DEFAULT FALSE,
    retry_policy VARCHAR(250) NULL,
    retry_attempt INTEGER NULL,
    job_data BYTEA NULL,
    PRIMARY KEY (sched_name, trigger_name, trigger_group),
    FOREIGN KEY (sched_name, job_name, job_group)
      REFERENCES qrtz_job_details (sched_name, job_name, job_group)
);

CREATE TABLE qrtz_simple_triggers
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    repeat_count BIGINT NOT NULL,
    repeat_interval BIGINT NOT NULL,
    times_triggered BIGINT NOT NULL,
    PRIMARY KEY (sched_name, trigger_name, trigger_group),
    FOREIGN KEY (sched_name, trigger_name, trigger_group)
      REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
      ON DELETE CASCADE
);

CREATE TABLE qrtz_simprop_triggers
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    str_prop_1 TEXT NULL,
    str_prop_2 TEXT NULL,
    str_prop_3 TEXT NULL,
    int_prop_1 INTEGER NULL,
    int_prop_2 INTEGER NULL,
    long_prop_1 BIGINT NULL,
    long_prop_2 BIGINT NULL,
    dec_prop_1 NUMERIC NULL,
    dec_prop_2 NUMERIC NULL,
    bool_prop_1 BOOL NULL,
    bool_prop_2 BOOL NULL,
    time_zone_id TEXT NULL,
    PRIMARY KEY (sched_name, trigger_name, trigger_group),
    FOREIGN KEY (sched_name, trigger_name, trigger_group)
      REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
      ON DELETE CASCADE
);

CREATE TABLE qrtz_cron_triggers
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    cron_expression TEXT NOT NULL,
    time_zone_id TEXT,
    PRIMARY KEY (sched_name, trigger_name, trigger_group),
    FOREIGN KEY (sched_name, trigger_name, trigger_group)
      REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
      ON DELETE CASCADE
);

CREATE TABLE qrtz_blob_triggers
  (
    sched_name TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    blob_data BYTEA NULL,
    PRIMARY KEY (sched_name, trigger_name, trigger_group),
    FOREIGN KEY (sched_name, trigger_name, trigger_group)
      REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
      ON DELETE CASCADE
);

CREATE TABLE qrtz_calendars
  (
    sched_name TEXT NOT NULL,
    calendar_name TEXT NOT NULL,
    calendar BYTEA NOT NULL,
    PRIMARY KEY (sched_name, calendar_name)
);

CREATE TABLE qrtz_paused_trigger_grps
  (
    sched_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    PRIMARY KEY (sched_name, trigger_group)
);

CREATE TABLE qrtz_paused_job_grps
  (
    sched_name TEXT NOT NULL,
    job_group TEXT NOT NULL,
    PRIMARY KEY (sched_name, job_group)
);

CREATE TABLE qrtz_fired_triggers
  (
    sched_name TEXT NOT NULL,
    entry_id TEXT NOT NULL,
    trigger_name TEXT NOT NULL,
    trigger_group TEXT NOT NULL,
    instance_name TEXT NOT NULL,
    fired_time BIGINT NOT NULL,
    sched_time BIGINT NOT NULL,
    priority INTEGER NOT NULL,
    state TEXT NOT NULL,
    job_name TEXT NULL,
    job_group TEXT NULL,
    is_nonconcurrent BOOL NOT NULL,
    requests_recovery BOOL NULL,
    execution_group VARCHAR(200) NULL,
    PRIMARY KEY (sched_name, entry_id)
);

CREATE TABLE qrtz_scheduler_state
  (
    sched_name TEXT NOT NULL,
    instance_name TEXT NOT NULL,
    last_checkin_time BIGINT NOT NULL,
    checkin_interval BIGINT NOT NULL,
    PRIMARY KEY (sched_name, instance_name)
);

CREATE TABLE qrtz_locks
  (
    sched_name TEXT NOT NULL,
    lock_name TEXT NOT NULL,
    PRIMARY KEY (sched_name, lock_name)
);

CREATE INDEX idx_qrtz_j_g_n ON qrtz_job_details (sched_name, job_group, job_name);
CREATE INDEX idx_qrtz_t_j ON qrtz_triggers (sched_name, job_name, job_group);
CREATE INDEX idx_qrtz_t_c ON qrtz_triggers (sched_name, calendar_name);
CREATE INDEX idx_qrtz_t_g_n ON qrtz_triggers (sched_name, trigger_group, trigger_name);
CREATE INDEX idx_qrtz_t_nft_st ON qrtz_triggers (sched_name, trigger_state, next_fire_time asc, priority desc, misfire_instr);
CREATE INDEX idx_qrtz_ft_inst_job_req_rcvry ON qrtz_fired_triggers (sched_name, instance_name, requests_recovery);
CREATE INDEX idx_qrtz_ft_j_g ON qrtz_fired_triggers (sched_name, job_name, job_group);
CREATE INDEX idx_qrtz_ft_t_g ON qrtz_fired_triggers (sched_name, trigger_name, trigger_group);

-- EF appends its migrations-history bookkeeping to this same transaction,
-- so the search_path must go back to the default before the SQL block ends.
SET LOCAL search_path TO public;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260907182843_AddQuartzSchema', '9.0.4');

CREATE TABLE core.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer NOT NULL DEFAULT 0,
    last_error text,
    CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id)
);

CREATE TABLE core.processed_domain_events (
    event_id uuid NOT NULL,
    handler_name character varying(200) NOT NULL,
    processed_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_processed_domain_events PRIMARY KEY (event_id, handler_name)
);

CREATE INDEX ix_domain_event_outbox_pending ON core.domain_event_outbox (occurred_on) WHERE dispatched_at IS NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260909134455_AddDomainEventOutboxAndProcessedEvents', '9.0.4');

ALTER TABLE core.files ADD claimed_at timestamp with time zone;

CREATE INDEX ix_files_created_at ON core.files (created_at) WHERE claimed_at IS NULL AND is_deleted = false;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260910132223_AddFileClaimedAt', '9.0.4');

COMMIT;

