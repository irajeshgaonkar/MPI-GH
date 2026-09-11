CREATE INDEX IF NOT EXISTS ix_client_identity_link_id_created
ON coalitionmpi.client_identity (mpi_link_id, created_date, client_identity_id);

CREATE INDEX IF NOT EXISTS ix_client_identity_created_date
ON coalitionmpi.client_identity (created_date);

DROP MATERIALIZED VIEW IF EXISTS coalitionmpi.mv_report_link_id_ingest_trend;

CREATE MATERIALIZED VIEW coalitionmpi.mv_report_link_id_ingest_trend AS
WITH classified AS (
    SELECT
        ci.created_date::date AS ingest_date,
        COALESCE(NULLIF(BTRIM(ci.source_system_name), ''), 'Unknown') AS source_system_name,
        CASE
            WHEN ROW_NUMBER() OVER (
                PARTITION BY ci.mpi_link_id
                ORDER BY ci.created_date, ci.client_identity_id
            ) = 1 THEN 1
            ELSE 0
        END AS is_new_link_id
    FROM coalitionmpi.client_identity ci
    WHERE NULLIF(BTRIM(ci.mpi_link_id), '') IS NOT NULL
)
SELECT
    ingest_date,
    source_system_name,
    COUNT(*)::integer AS incoming_records,
    COUNT(*) FILTER (WHERE is_new_link_id = 1)::integer AS new_person_records,
    COUNT(*) FILTER (WHERE is_new_link_id = 0)::integer AS already_in_mpi_records
FROM classified
GROUP BY ingest_date, source_system_name;

ALTER MATERIALIZED VIEW coalitionmpi.mv_report_link_id_ingest_trend OWNER TO svcmpidbprodhca;

CREATE UNIQUE INDEX ux_mv_report_link_id_ingest_trend
ON coalitionmpi.mv_report_link_id_ingest_trend (ingest_date, source_system_name);

CREATE INDEX ix_mv_report_link_id_ingest_trend_lookup
ON coalitionmpi.mv_report_link_id_ingest_trend (ingest_date, source_system_name);
