using HCA.Core.Mapper;
using HCA.Data;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Request;

namespace HCA.Core.Processors
{
    public class FileDataLoader : IFileDataLoader
    {
        private readonly ILogger _logger;

        private readonly IFileParser _fileParser;

        private readonly IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> _clientIdentityRequestMapper;

        private readonly IFileRequestRepository _fileRequestRepository;

        private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

        private readonly IRequestProcessLogRepository _requestProcessLogRepository;

        public FileDataLoader(ILogger logger, IFileParser fileParser, IClientIdentityRequestRepository clientIdentityRequestRepository,
            IFileRequestRepository fileRequestRepository, IRequestProcessLogRepository requestProcessLogRepository,
            IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper)
        {
            _logger = logger;
            _fileParser = fileParser;
            _clientIdentityRequestMapper = clientIdentityRequestMapper;
            _clientIdentityRequestRepository = clientIdentityRequestRepository;
            _requestProcessLogRepository = requestProcessLogRepository;
            _fileRequestRepository = fileRequestRepository;
        }

        public async Task ProcessFile(string fileName, StreamReader stream)
        {
            LogInformation($"Started File Parsing file {fileName}");
            var fileRequest = await CreateFileRequest(fileName);
            await LogInfo(fileRequest.RequestId, "Started File Parsing");

            try
            {
                var (clientIdentityRequests, errorMeesages, headerData) = _fileParser.ParseFile(stream);
                var clientIdentityRequestsList = clientIdentityRequests.ToList();

                if (HasErrors(clientIdentityRequests))
                {
                    fileRequest.Status = DataConstants.Statuses.Failed;
                    fileRequest.Message = $"Error parsing the records - {errorMeesages.CombineToString()}";
                    await _fileRequestRepository.Update(fileRequest);
                    await LogInfo(fileRequest.RequestId, "Error parsing the records - {errorMeesage}");
                    return;
                }

                for(int i = 0; i < clientIdentityRequestsList.Count; ++i)
                {
                    var clientIndentityRequest = clientIdentityRequestsList[i];
                    clientIndentityRequest.Status = errorMeesages.ContainsKey(i + 3)
                                                        ? DataConstants.Statuses.Failed
                                                        : DataConstants.Statuses.NotStarted;
                    clientIndentityRequest.Message = errorMeesages.ContainsKey(i + 3)
                                                        ? errorMeesages[i + 3]
                                                        : string.Empty;
                    clientIndentityRequest.RequestId = fileRequest.RequestId;
                    clientIndentityRequest.TrackingId = Guid.NewGuid().ToString();
                }

                await LogInfo(fileRequest.RequestId, "Started Loading Data into Database");
                var entities = _clientIdentityRequestMapper.MapToEntityCollection(clientIdentityRequests);
                await _clientIdentityRequestRepository.InsertBulk(entities);
                await LogInfo(fileRequest.RequestId, "Completed Loading Data into Database");

                fileRequest.Status = headerData.OperationType;
                fileRequest.Message = string.Empty;
                fileRequest.RecordsCount = clientIdentityRequestsList.Count;
                fileRequest.OperationType = headerData.OperationType;
                await _fileRequestRepository.Update(fileRequest);

                LogInformation($"Completed File Parsing and loading data into database for file {fileName}");
            }
            catch(Exception ex)
            {
                fileRequest.Status = DataConstants.Statuses.ParsingFailed;
                fileRequest.Message = $"Error parsing the records - {ex}";
                await _fileRequestRepository.Update(fileRequest);
            }
        }

        private async Task LogInfo(Guid requestId, string message)
        {
            LogInformation(message, requestId.ToString());
            await LogInDatabase(requestId, message);
        }

        private void LogInformation(string message, string requestId = "")
        {
            _logger.LogInformation($"FileDataLoader: request Id {requestId} => {message}");
        }

        private async Task LogInDatabase(Guid requestId, string message)
        {
            await _requestProcessLogRepository.LogStatus(requestId, message);
        }

        private bool HasErrors(IEnumerable<ClientIdentityRequest> clientIdentities)
        {
            return null == clientIdentities || !clientIdentities.Any();
        }

        private async Task<FileRequestEntity> CreateFileRequest(string fileName)
        {
            FileRequestEntity fileRequest = new()
            {
                RequestId = Guid.NewGuid(),
                FileName = fileName,
                RecordsCount = 0,
                OperationType = string.Empty,
                RequestDateTime = DateTime.Now,
                Status = DataConstants.Statuses.ParsingFile,
                Message = string.Empty
            };

            await _fileRequestRepository.Insert(fileRequest);
            return fileRequest;
        }
    }
}

