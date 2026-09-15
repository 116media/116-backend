CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'identity') THEN
        CREATE SCHEMA identity;
    END IF;
END $EF$;

CREATE TABLE identity.permissions (
    id uuid NOT NULL,
    resource character varying(15) NOT NULL,
    action character varying(15) NOT NULL,
    description character varying(300) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_permissions PRIMARY KEY (id)
);

CREATE TABLE identity.roles (
    id uuid NOT NULL,
    name character varying(20) NOT NULL,
    description character varying(300) NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_roles PRIMARY KEY (id)
);

CREATE TABLE identity.users (
    id uuid NOT NULL,
    email character varying(254),
    user_name character varying(20) NOT NULL,
    password_hash text,
    auth_provider text NOT NULL,
    is_verified boolean NOT NULL DEFAULT FALSE,
    is_active boolean NOT NULL DEFAULT TRUE,
    avatar_file_id uuid,
    avatar_source text NOT NULL,
    country_name character varying(100),
    country_iso_code character varying(3),
    country_dial_code character varying(10),
    partial_phone_number character varying(50),
    full_phone_number character varying(20),
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_users PRIMARY KEY (id)
);

CREATE TABLE identity.role_permissions (
    id uuid NOT NULL,
    role_id uuid NOT NULL,
    permission_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_role_permissions PRIMARY KEY (id),
    CONSTRAINT fk_role_permissions_permissions_permission_id FOREIGN KEY (permission_id) REFERENCES identity.permissions (id) ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_roles_role_id FOREIGN KEY (role_id) REFERENCES identity.roles (id) ON DELETE CASCADE
);

CREATE TABLE identity.otps (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    code character varying(6) NOT NULL,
    purpose text NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    attempt_count integer NOT NULL DEFAULT 0,
    is_used boolean NOT NULL DEFAULT FALSE,
    used_at timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_otps PRIMARY KEY (id),
    CONSTRAINT fk_otps_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

CREATE TABLE identity.sessions (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    device_id character varying(64) NOT NULL,
    refresh_token_hash character varying(500) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    ip_address character varying(45),
    user_agent character varying(500),
    browser text NOT NULL,
    device text NOT NULL,
    platform text NOT NULL,
    client text NOT NULL,
    is_revoked boolean NOT NULL DEFAULT FALSE,
    revoked_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_sessions PRIMARY KEY (id),
    CONSTRAINT fk_sessions_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

CREATE TABLE identity.user_roles (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_user_roles PRIMARY KEY (id),
    CONSTRAINT fk_user_roles_roles_role_id FOREIGN KEY (role_id) REFERENCES identity.roles (id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

CREATE INDEX "IX_Otps_ExpiresAt" ON identity.otps (expires_at);

CREATE INDEX "IX_Otps_Purpose_ExpiresAt" ON identity.otps (purpose, expires_at);

CREATE INDEX "IX_Otps_UserId_Code_Purpose" ON identity.otps (user_id, code, purpose);

CREATE INDEX "IX_Otps_UserId_Purpose" ON identity.otps (user_id, purpose);

CREATE UNIQUE INDEX "IX_permissions_resource_action" ON identity.permissions (resource, action);

CREATE INDEX ix_role_permissions_permission_id ON identity.role_permissions (permission_id);

CREATE UNIQUE INDEX "IX_role_permissions_role_id_permission_id" ON identity.role_permissions (role_id, permission_id);

CREATE UNIQUE INDEX ix_roles_name ON identity.roles (name);

CREATE INDEX ix_sessions_device_id ON identity.sessions (device_id);

CREATE INDEX ix_sessions_expires_at ON identity.sessions (expires_at);

CREATE INDEX ix_sessions_is_revoked ON identity.sessions (is_revoked);

CREATE INDEX ix_sessions_refresh_token_hash ON identity.sessions (refresh_token_hash);

CREATE INDEX ix_sessions_user_id ON identity.sessions (user_id);

CREATE UNIQUE INDEX ix_sessions_user_id_device_id ON identity.sessions (user_id, device_id) WHERE is_revoked = false;

CREATE INDEX ix_user_roles_role_id ON identity.user_roles (role_id);

CREATE UNIQUE INDEX "IX_user_roles_user_id_role_id" ON identity.user_roles (user_id, role_id);

CREATE UNIQUE INDEX ix_users_email ON identity.users (email);

CREATE UNIQUE INDEX ix_users_user_name ON identity.users (user_name);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260111143151_InitIdentitySchema', '9.0.4');

ALTER TABLE identity.roles ADD deleted_at timestamp with time zone;

ALTER TABLE identity.roles ADD is_active boolean NOT NULL DEFAULT TRUE;

ALTER TABLE identity.roles ADD is_deleted boolean NOT NULL DEFAULT FALSE;

ALTER TABLE identity.permissions ADD deleted_at timestamp with time zone;

ALTER TABLE identity.permissions ADD is_active boolean NOT NULL DEFAULT TRUE;

ALTER TABLE identity.permissions ADD is_deleted boolean NOT NULL DEFAULT FALSE;

CREATE INDEX "IX_roles_is_active" ON identity.roles (is_active);

CREATE INDEX "IX_roles_is_deleted" ON identity.roles (is_deleted);

CREATE INDEX "IX_permissions_is_active" ON identity.permissions (is_active);

CREATE INDEX "IX_permissions_is_deleted" ON identity.permissions (is_deleted);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260124173824_AddRolePermissionStatusFields', '9.0.4');

DROP INDEX identity."IX_Otps_UserId_Code_Purpose";

DELETE FROM identity.otps;

ALTER TABLE identity.otps RENAME COLUMN code TO code_hash;

ALTER TABLE identity.otps ALTER COLUMN code_hash TYPE character varying(100);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260819133153_HashOtpCodeAtRest', '9.0.4');

ALTER TABLE identity.users ADD provider_subject_id character varying(255);

CREATE UNIQUE INDEX ix_users_auth_provider_provider_subject_id ON identity.users (auth_provider, provider_subject_id) WHERE provider_subject_id IS NOT NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260828173608_AddUserProviderSubjectId', '9.0.4');

CREATE TABLE identity.user_token_state (
    user_id uuid NOT NULL,
    security_stamp uuid NOT NULL,
    token_version bigint NOT NULL DEFAULT 0,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_user_token_state PRIMARY KEY (user_id),
    CONSTRAINT fk_user_token_state_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

INSERT INTO identity.user_token_state (user_id, security_stamp, token_version, created_at)
SELECT id, gen_random_uuid(), 0, now()
FROM identity.users;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260829175355_AddUserTokenState', '9.0.4');

ALTER TABLE identity.sessions ADD absolute_expires_at timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

UPDATE identity.sessions
SET absolute_expires_at = created_at + INTERVAL '30 days';

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260829175502_AddSessionAbsoluteExpiresAt', '9.0.4');

ALTER TABLE identity.users ADD failed_login_attempts integer NOT NULL DEFAULT 0;

ALTER TABLE identity.users ADD locked_until timestamp with time zone;

ALTER TABLE identity.otps ADD consumed_at timestamp with time zone;

CREATE TABLE identity.user_otp_state (
    user_id uuid NOT NULL,
    failed_attempts integer NOT NULL DEFAULT 0,
    locked_until timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_user_otp_state PRIMARY KEY (user_id),
    CONSTRAINT fk_user_otp_state_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

DELETE FROM identity.otps;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260902103008_AddAccountLockoutAndOtpConsumption', '9.0.4');

CREATE TABLE identity.domain_event_outbox (
    id uuid NOT NULL,
    event_type character varying(500) NOT NULL,
    payload text NOT NULL,
    occurred_on timestamp with time zone NOT NULL,
    dispatched_at timestamp with time zone,
    attempt_count integer NOT NULL DEFAULT 0,
    last_error text,
    CONSTRAINT pk_domain_event_outbox PRIMARY KEY (id)
);

CREATE TABLE identity.processed_domain_events (
    event_id uuid NOT NULL,
    handler_name character varying(200) NOT NULL,
    processed_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_processed_domain_events PRIMARY KEY (event_id, handler_name)
);

CREATE INDEX ix_domain_event_outbox_pending ON identity.domain_event_outbox (occurred_on) WHERE dispatched_at IS NULL;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260909134504_AddDomainEventOutboxAndProcessedEvents', '9.0.4');

DROP TABLE identity.processed_domain_events;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260910172740_DropRedundantProcessedDomainEvents', '9.0.4');

CREATE TABLE identity.user_login_state (
    user_id uuid NOT NULL,
    failed_attempts integer NOT NULL DEFAULT 0,
    locked_until timestamp with time zone,
    created_at timestamp with time zone,
    created_by text,
    updated_at timestamp with time zone,
    updated_by text,
    CONSTRAINT pk_user_login_state PRIMARY KEY (user_id),
    CONSTRAINT fk_user_login_state_users_user_id FOREIGN KEY (user_id) REFERENCES identity.users (id) ON DELETE CASCADE
);

INSERT INTO identity.user_login_state (user_id, failed_attempts, locked_until, created_at)
SELECT id, failed_login_attempts, locked_until, now()
FROM identity.users
WHERE failed_login_attempts <> 0 OR locked_until IS NOT NULL;

ALTER TABLE identity.users DROP COLUMN failed_login_attempts;

ALTER TABLE identity.users DROP COLUMN locked_until;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260914052728_AddUserLoginState', '9.0.4');

COMMIT;

