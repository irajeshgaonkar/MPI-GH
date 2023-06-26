-- Table: coalitionmpi.custom_data_mapping

-- DROP TABLE IF EXISTS coalitionmpi.custom_data_mapping;

CREATE TABLE IF NOT EXISTS coalitionmpi.custom_data_mapping
(
    id integer NOT NULL DEFAULT nextval('coalitionmpi.custom_data_mapping_id_seq'::regclass),
    source_system_name character varying(1000) COLLATE pg_catalog."default",
    input_index integer,
    input_column_name character varying(3000) COLLATE pg_catalog."default",
    verato_request_path character varying(3000) COLLATE pg_catalog."default",
    verato_response_path character varying(3000) COLLATE pg_catalog."default",
    api_response_path character varying(3000) COLLATE pg_catalog."default",
    output_index integer,
    output_column_name character varying(3000) COLLATE pg_catalog."default",
    CONSTRAINT custom_data_mapping_pkey PRIMARY KEY (id)
)

	 DO $$
  BEGIN
	  IF (select count(*) from coalitionmpi.custom_data_mapping) = 0
	  THEN	 
		INSERT INTO coalitionmpi.custom_data_mapping(
		source_system_name, input_index, input_column_name, verato_request_path, verato_response_path, api_response_path, output_index, output_column_name)
		VALUES (
			'wadoh.wdrs',
			0, 
			'createdDate12',
			'custom.data$createdDate',
			'custom.data$createdDate', 
			'custom.data$createdDate', 
			0, 
			'createdDate'
				);
	  END IF;
  END $$;
  


-- Table: coalitionmpi.service_accounts

-- DROP TABLE IF EXISTS coalitionmpi.service_accounts;

CREATE TABLE IF NOT EXISTS coalitionmpi.service_accounts
(
    id integer NOT NULL DEFAULT nextval('coalitionmpi.service_accounts_id_seq'::regclass),
    source_system_name character varying(1000) COLLATE pg_catalog."default",
    app_id character varying(1000) COLLATE pg_catalog."default",
    CONSTRAINT service_accounts_pkey PRIMARY KEY (id)
)


	 DO $$
  BEGIN
	  IF (select count(*) from coalitionmpi.service_accounts) = 0
	  THEN
		 INSERT INTO coalitionmpi.service_accounts(
 		source_system_name, app_id)
		VALUES (
			'WAHCA.Providerone', 
			'99be4864-532b-4432-9a6c-2f364a10532d'
	      	 	);
	  END IF;
  END $$;


  Alter Table coalitionmpi.client_identity_requests
  Add custom_json text;
  
  Alter Table coalitionmpi.client_identity 
  Add custom_json text;