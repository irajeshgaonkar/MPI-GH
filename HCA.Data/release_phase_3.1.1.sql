	-- update custom_data_mapping table values with new requested naming convention 
	 
	 DO $$
  BEGIN
	  IF (select id from coalitionmpi.custom_data_mapping where id = 1) = 1
	  THEN	 
		update coalitionmpi.custom_data_mapping set input_column_name = 'createDate'
		, verato_request_path = 'custom.CreateDates$createdDate'
		, verato_response_path = 'custom.CreateDates$createdDate'
		, api_response_path = 'custom.CreateDates$createdDate'
		, output_column_name = 'createDate'
		Where id = 1;
	  END IF;
  END $$;