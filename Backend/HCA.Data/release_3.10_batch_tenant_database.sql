ALTER TABLE coalitionmpi.sftp_file_transfer
ADD COLUMN IF NOT EXISTS tenant character varying(40) NOT NULL DEFAULT 'HHS Coalition';

COMMENT ON COLUMN coalitionmpi.sftp_file_transfer.tenant
IS 'Database tenant selected from the inbound SFTP root path.';

ALTER TABLE coalitionmpi.file_requests
ADD COLUMN IF NOT EXISTS tenant character varying(40) NOT NULL DEFAULT 'HHS Coalition';

COMMENT ON COLUMN coalitionmpi.file_requests.tenant
IS 'Database tenant used for batch processing and output generation.';

UPDATE coalitionmpi.sftp_file_transfer
SET tenant = CASE
    WHEN tenant = 'NonCoalition' THEN 'Non-Coalition'
    WHEN tenant = 'Coalition' THEN 'HHS Coalition'
    ELSE tenant
END;

UPDATE coalitionmpi.file_requests
SET tenant = CASE
    WHEN tenant = 'NonCoalition' THEN 'Non-Coalition'
    WHEN tenant = 'Coalition' THEN 'HHS Coalition'
    ELSE tenant
END;
