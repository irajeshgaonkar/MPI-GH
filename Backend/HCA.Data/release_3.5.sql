CREATE INDEX client_identity_requests_request_id
ON coalitionmpi.client_identity_requests (request_id);

CREATE TABLE coalitionmpi.client_identity_requests_history (LIKE coalitionmpi.client_identity_requests INCLUDING ALL);

-- Un Comment and run the lines for moving all the data from requests table to history table
--INSERT INTO coalitionmpi.client_identity_requests_history
--SELECT * from coalitionmpi.client_identity_requests

