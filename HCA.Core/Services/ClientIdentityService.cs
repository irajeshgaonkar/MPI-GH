using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Sqs;
using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Request;
using HCA.Models.MuleSoft.Response;
using HCA.Models.Request;
using HCA.Models.Request.DOH;
using HCA.Models.Response;
using HCA.Models.SQS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace HCA.Core.Services;

public class ClientIdentityService : IClientIdentityService
{
    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IUserModifyRecordsService _userModifyRecordsService;

    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    private readonly IUserModifyRecordsRepository _userModifyRecordsRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    private readonly ISqsPublisher _sqsPublisher;

    private readonly IUserRequestMapper _userRequestMapper;

    public ClientIdentityService(IUserRequestRepository userRequestRepository,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor,
        IClientIdentityRepository clientIdentityRepository,
        IUserModifyRecordsRepository userModifyRecordsRepository,
        IRequestProcessLogRepository requestProcessLogRepository,
        IUserModifyRecordsService userModifyRecordsService,
        ISqsPublisher sqsPublisher, IUserRequestMapper userRequestMapper)
    {
        _userRequestRepository = userRequestRepository;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
        _clientIdentityRepository = clientIdentityRepository;
        _userModifyRecordsRepository = userModifyRecordsRepository;
        _requestProcessLogRepository = requestProcessLogRepository;
        _sqsPublisher = sqsPublisher;
        _userRequestMapper = userRequestMapper;
        _userModifyRecordsService = userModifyRecordsService;
    }

    public async Task<(int, IEnumerable<ClientIdentityModel>)> GetAll(string currentUser, Dictionary<string, string> searchFilter, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "")
    {
        //var userModifyRecords = (await _userModifyRecordsRepository.GetAllAsync(r => r.UserName == currentUser)).Select(t => t.ClientIdentityId).ToList();
        var userModifyRecords = new List<int>();
        var (count, entities) = await _clientIdentityRepository.GetAll(searchFilter, userModifyRecords, pageNumber, recordsPerPage, orderBy);
        var models = ClientIdentityMapper.MapToClientIdentityModel(entities);
        return (count, models);
    }

    public async Task<dynamic?> PostIdentities(IEnumerable<ClientIdentityRequest> identities, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEPost.GetStringValue()}-{identities.First().SourceSystemName}-{identities.First().SourceSystemName}";
        var userRequestEntity = CreateUserRequest(identities, ApiCallType.VELink, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VELink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await PostIdentities(userRequestEntity, identities);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_PostIdentities(DOH_PostClientIdentityRequest request, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {

        string strIdentities = request.Content.Identity.ToString();
        Identity identity = JsonConvert.DeserializeObject<Identity>(strIdentities);

        var trackingId = $"{ApiCallType.DOH_VEPost.GetStringValue()}-{identity.Sources.First().Name}-{identity.Sources.First().Name}-{ClientIdentityRequestExtension.GetTrackingId()}";


        DOH_PostClientIdentityRequest dOH_PostClientIdentityRequest = new DOH_PostClientIdentityRequest(trackingId);
        if (strIdentities.ToLower().Contains("null"))
        {
            // Replace null values with empty strings and get modified JSON string 
            dynamic modifiedJson = ReplaceNullValues(request.Content.Identity.ToString());

            JsonElement modifiedJsonElement = ConvertJObjectToJsonElement(modifiedJson);

            DOH_PostIdentityRequestContent postIdentityRequestContent = new DOH_PostIdentityRequestContent(modifiedJsonElement);

            postIdentityRequestContent.ResponseIdentityFormatNames = request.Content.ResponseIdentityFormatNames;
            dOH_PostClientIdentityRequest.Content = postIdentityRequestContent;
        }
        else
        {
            dOH_PostClientIdentityRequest.Content = request.Content;
        }


        var userRequestEntity = CreateUserRequest(dOH_PostClientIdentityRequest, ApiCallType.DOH_VEPost, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VELink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await DOH_PostIdentities(userRequestEntity, dOH_PostClientIdentityRequest);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    private JsonElement ConvertJObjectToJsonElement(JObject jObject)
    {
        // Serialize the JObject to a JSON string
        string jsonString = jObject.ToString();

        // Parse the JSON string into a JsonDocument
        using (JsonDocument document = JsonDocument.Parse(jsonString))
        {
            // Get the root element of the JsonDocument
            JsonElement rootElement = document.RootElement;

            // Return the root element
            return rootElement.Clone();
        }
    }



    public JObject ReplaceNullValues(string jsonString)
    {
        JObject jsonObject = JObject.Parse(jsonString);
        ReplaceNullValues(jsonObject);
        return jsonObject;
    }

    private void ReplaceNullValues(JObject obj)
    {
        foreach (var property in obj.Properties())
        {
            if (property.Value.Type == JTokenType.Null)
            {
                property.Value = "";
            }
            else if (property.Value.Type == JTokenType.Object)
            {
                ReplaceNullValues((JObject)property.Value);
            }
            else if (property.Value.Type == JTokenType.Array)
            {
                JArray array = (JArray)property.Value;
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].Type == JTokenType.Null)
                    {
                        array[i] = "";
                    }
                    else if (array[i].Type == JTokenType.Object)
                    {
                        ReplaceNullValues((JObject)array[i]);
                    }
                }
            }

        }
    }


    public async Task<dynamic?> LinkIdentities(LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEUnLink.GetStringValue()}-{linkingSources.Source.GetTrackingId(linkingSources.LinkToSource)}";
        var userRequestEntity = CreateUserRequest(linkingSources, ApiCallType.VELink, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VELink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await LinkIdentities(userRequestEntity, linkingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> UnLinkIdentities(UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEUnLink.GetStringValue()}-{unLinkingSources.Source.GetTrackingId(unLinkingSources.UnlinkFromSource)}";
        var userRequestEntity = CreateUserRequest(unLinkingSources, ApiCallType.VEUnLink, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEUnLink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unLinkingSources.UnlinkFromSource, unLinkingSources.Source);
            return await UnLinkIdentities(userRequestEntity, unLinkingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> MergeIdentities(MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEMerge.GetStringValue()}-{mergingSources.ToSurviveSource.GetTrackingId(mergingSources.ToRetireSource)}";
        var userRequestEntity = CreateUserRequest(mergingSources, ApiCallType.VEMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);
            return await MergeIdentities(userRequestEntity, mergingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> UnMergeIdentities(UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEUnMerge.GetStringValue()}-{unMergingSources.UnmergeSource.GetTrackingId(unMergingSources.UnmergeSource)}";
        var userRequestEntity = CreateUserRequest(unMergingSources, ApiCallType.VEUnMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEUnMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unMergingSources.UnmergeSource, unMergingSources.UnmergeFromSource);
            return await UnMergeIdentities(userRequestEntity, unMergingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DemographicSearch(Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEDemographicSearch.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
        var userRequestEntity = CreateUserRequest(filter, ApiCallType.VEDemographicSearch, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEDemographicSearch, userRequestEntity);
                return trackingId;
            }

            return await DemographicSearch(userRequestEntity, filter);
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DemographicQuery(Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions) //,string responseIdentityFormatNames = "DEFAULT")
    {
        var trackingId = $"{ApiCallType.VEDemographicQuery.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
        var userRequestEntity = CreateUserRequest(filter, ApiCallType.VEDemographicQuery, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEDemographicQuery, userRequestEntity);
                return trackingId;
            }

            return await DemographicQuery(userRequestEntity, filter);
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_DemographicSearch(DOH_DemographicQueryRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEDemographicSearch.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";

        if (filter.content.responseIdentityFormatNames == null || (filter.content.responseIdentityFormatNames != null && filter.content.responseIdentityFormatNames[0] == ""))
        {
            filter.content.responseIdentityFormatNames = new string[] { "DEFAULT" };
        }

        string strIdentities = filter.content.identity.ToString();

        DOH_DemographicQueryRequest dOH_DemographicQueryRequest = new DOH_DemographicQueryRequest();
        if (strIdentities.ToLower().Contains("null"))
        {
            // Replace null values with empty strings and get modified JSON string 
            dynamic modifiedJson = ReplaceNullValues(filter.content.identity.ToString());

            JsonElement modifiedJsonElement = ConvertJObjectToJsonElement(modifiedJson);

            Content content = new Content();
            content.identity = modifiedJsonElement;
            content.responseIdentityFormatNames = filter.content.responseIdentityFormatNames;

            dOH_DemographicQueryRequest.content = content;
        }
        else
        {
            dOH_DemographicQueryRequest.content = filter.content;
        }

        var userRequestEntity = CreateUserRequest(dOH_DemographicQueryRequest, ApiCallType.VEDemographicSearch, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEDemographicSearch, userRequestEntity);
                return trackingId;
            }

            return await DOH_DemographicSearch(userRequestEntity, dOH_DemographicQueryRequest);
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_DemographicQuery(DOH_DemographicQueryRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions) //,string responseIdentityFormatNames = "DEFAULT")
    {
        var  trackingId = $"{ApiCallType.DOH_VEDemographicQuery.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";

        if (filter.content.responseIdentityFormatNames == null || (filter.content.responseIdentityFormatNames != null && filter.content.responseIdentityFormatNames[0] == ""))
        {
            filter.content.responseIdentityFormatNames = new string[] { "DEFAULT" };
        }

        string strIdentities = filter.content.identity.ToString();

        DOH_DemographicQueryRequest dOH_DemographicQueryRequest = new DOH_DemographicQueryRequest();
        if (strIdentities.ToLower().Contains("null"))
        {
            // Replace null values with empty strings and get modified JSON string 
            dynamic modifiedJson = ReplaceNullValues(filter.content.identity.ToString());

            JsonElement modifiedJsonElement = ConvertJObjectToJsonElement(modifiedJson);

            Content content = new Content();
            content.identity = modifiedJsonElement;
            content.responseIdentityFormatNames = filter.content.responseIdentityFormatNames;

            dOH_DemographicQueryRequest.content = content;
        }
        else
        {
            dOH_DemographicQueryRequest.content = filter.content;
        }


        var userRequestEntity = CreateUserRequest(dOH_DemographicQueryRequest, ApiCallType.DOH_VEDemographicQuery, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEDemographicQuery, userRequestEntity);
                return trackingId;
            }

            return await DOH_DemographicQuery(userRequestEntity, dOH_DemographicQueryRequest);
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_LinkIdentities(DOH_LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = string.Empty;
        if (linkingSources.trackingId.ToString().Length >= 1)
        {
            trackingId = linkingSources.trackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VELink.GetStringValue()}-{linkingSources.content.Source.GetTrackingId(linkingSources.content.LinkToSource)}";
        }
        var userRequestEntity = CreateUserRequest(linkingSources, ApiCallType.DOH_VELink, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VELink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await DOH_LinkIdentities(userRequestEntity, linkingSources.content);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_UnLinkIdentities(DOH_UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = string.Empty;
        if (unLinkingSources.trackingId.ToString().Length >= 1)
        {
            trackingId = unLinkingSources.trackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VEUnLink.GetStringValue()}-{unLinkingSources.content.Source.GetTrackingId(unLinkingSources.content.UnlinkFromSource)}";
        }
        var userRequestEntity = CreateUserRequest(unLinkingSources, ApiCallType.DOH_VEUnLink, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEUnLink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unLinkingSources.UnlinkFromSource, unLinkingSources.Source);
            return await DOH_UnLinkIdentities(userRequestEntity, unLinkingSources.content);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_MergeIdentities(DOH_MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = string.Empty;
        if (mergingSources.trackingId.ToString().Length >= 1)
        {
            trackingId = mergingSources.trackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VEMerge.GetStringValue()}-{mergingSources.content.ToSurviveSource.GetTrackingId(mergingSources.content.ToRetireSource)}";
        }
        var userRequestEntity = CreateUserRequest(mergingSources, ApiCallType.DOH_VEMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);
            return await DOH_MergeIdentities(userRequestEntity, mergingSources.content);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_UnMergeIdentities(DOH_UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = string.Empty;
        if (unMergingSources.trackingId.ToString().Length >= 1)
        {
            trackingId = unMergingSources.trackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VEUnMerge.GetStringValue()}-{unMergingSources.content.UnmergeSource.GetTrackingId(unMergingSources.content.UnmergeSource)}";
        }

        var userRequestEntity = CreateUserRequest(unMergingSources, ApiCallType.DOH_VEUnMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEUnMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unMergingSources.UnmergeSource, unMergingSources.UnmergeFromSource);
            return await DOH_UnMergeIdentities(userRequestEntity, unMergingSources.content);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DOH_DeleteSourceIdentity(DOH_DeleteClientIdentityRequest deleteSourceIdentity, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = string.Empty;
        if (deleteSourceIdentity.TrackingId.ToString().Length >= 1)
        {
            trackingId = deleteSourceIdentity.TrackingId.ToString();
        }
        else
        {
             trackingId = $"{ApiCallType.DOH_VEDelete.GetStringValue()}-{deleteSourceIdentity.Content.Source.GetTrackingId(deleteSourceIdentity.Content.Source)}";
        }
                var userRequestEntity = CreateUserRequest(deleteSourceIdentity, ApiCallType.DOH_VEDelete, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEDelete, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);

            return await DOH_DeleteSourceIdentity(userRequestEntity, deleteSourceIdentity);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    private async Task<PostIdentityResponseContent?> PostIdentities(UserRequestEntity userRequestEntity, IEnumerable<ClientIdentityRequest> identities)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new PostClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = identities.ToList()
        };

        var response = await _clientIdentityRequestExecutor.Execute<PostClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<dynamic?> DOH_PostIdentities(UserRequestEntity userRequestEntity, DOH_PostClientIdentityRequest request)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        if (request.Content.ResponseIdentityFormatNames == null || (request.Content.ResponseIdentityFormatNames != null && request.Content.ResponseIdentityFormatNames[0] == ""))
        {
            request.Content.ResponseIdentityFormatNames = new string[] { "DEFAULT" };
        }

        var linkIdentityRequest = new DOH_PostClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = request.Content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_PostClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task<LinkIdentitiesResponseContent?> LinkIdentities(UserRequestEntity userRequestEntity, LinkingSources linkingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new LinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = linkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<LinkClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<List<PostIdentityResponseContent>?> DemographicSearch(UserRequestEntity userRequestEntity, Identity filter)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicSearchClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicSearchClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<dynamic?> DOH_DemographicSearch(UserRequestEntity userRequestEntity, DOH_DemographicQueryRequest filter)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DOH_DemographicSearchClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter.content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_DemographicSearchClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task<DemographicQueryResponseContent?> DemographicQuery(UserRequestEntity userRequestEntity, Identity filter)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicQueryClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<dynamic?> DOH_DemographicQuery(UserRequestEntity userRequestEntity, DOH_DemographicQueryRequest filter)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DOH_DemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter.content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_DemographicQueryClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities(UserRequestEntity userRequestEntity, UnLinkingSources unLinkingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var unLinkClientIdentityRequest = new UnLinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unLinkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<UnLinkClientIdentityResponse>(unLinkClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<MergeIdentitiesResponseContent?> MergeIdentities(UserRequestEntity userRequestEntity, MergingSources mergingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var mergeClientIdentityRequest = new MergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = mergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<MergeClientIdentityResponse>(mergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities(UserRequestEntity userRequestEntity, UnMergingSources unMergingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var unMergeClientIdentityRequest = new UnMergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unMergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<UnMergeClientIdentityResponse>(unMergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<dynamic?> DOH_LinkIdentities(UserRequestEntity userRequestEntity, LinkingSources linkingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new DOH_LinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = linkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_LinkClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task<dynamic?> DOH_UnLinkIdentities(UserRequestEntity userRequestEntity, UnLinkingSources unLinkingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var unLinkClientIdentityRequest = new DOH_UnLinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unLinkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_UnLinkClientIdentityResponse>(unLinkClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task<dynamic?> DOH_MergeIdentities(UserRequestEntity userRequestEntity, MergingSources mergingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var mergeClientIdentityRequest = new DOH_MergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = mergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<DOH_MergeClientIdentityResponse>(mergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task<dynamic?> DOH_DeleteSourceIdentity(UserRequestEntity userRequestEntity, DOH_DeleteClientIdentityRequest deleteSourceIdentity)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var deleteClientIdentityRequest = new DOH_DeleteClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = deleteSourceIdentity.Content
        };
        var response = await _clientIdentityRequestExecutor.Execute<DOH_DeleteClientIdentityResponse>(deleteClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task<dynamic?> DOH_UnMergeIdentities(UserRequestEntity userRequestEntity, UnMergingSources unMergingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var unMergeClientIdentityRequest = new DOH_UnMergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unMergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<DOH_UnMergeClientIdentityResponse>(unMergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response;
    }

    private async Task PublishMessageToSqs(ApiCallType apiCallType, UserRequestEntity userRequestEntity)
    {
        var messageType = MessageType.UserRequest;
        if (messageType == null) throw new ArgumentException($"ClientIdentityService:PublishMessageToSqs: cannot process message for {apiCallType.GetStringValue()}");

        var userRequest = _userRequestMapper.MapToModel(userRequestEntity);
        var UserRequestMessage = new UserRequestMessage()
        {
            ApiCallType = apiCallType.GetStringValue(),
            UserRequest = userRequest
        };

        var sqsMessage = new SqsMessage()
        {
            MessageType = messageType,
            Payload = SerializationExtensions.Serialize(UserRequestMessage)
        };

        await _sqsPublisher.PublishMessage(sqsMessage);
    }

    private UserRequestEntity CreateUserRequest<T>(T request, ApiCallType apiCallType, string userName, string trackingId, NotificationOptions? notificationOptions)
    {
        if (request == null) throw new ArgumentNullException("ClientIdentityService:CreateUserRequest:Request cannot be nulle");

        var userRequest = new UserRequestEntity()
        {
            TrackingId = trackingId,
            UserName = userName,
            ApiCallType = apiCallType.GetStringValue(),
            RequestJson = SerializationExtensions.Serialize(request),
            ResponseJson = string.Empty,
            NotificationOptions = null == notificationOptions ? string.Empty : SerializationExtensions.Serialize(notificationOptions),
            RequestDateTime = DateTime.Now,
            ProcessStartTime = DateTime.Now,
            Status = RequestStatus.Processing.GetStringValue(),
            Message = string.Empty,
            RetryCount = 0
        };

        _userRequestRepository.AddAsync(userRequest);
        return userRequest;
    }

    private void UpdateProcessStatus(UserRequestEntity userRequest, RequestStatus status, string message)
    {
        userRequest.Status = status.GetStringValue();
        userRequest.Message = message;
        userRequest.ProcessEndTime = DateTime.Now;
        _userRequestRepository.Update(userRequest);
    }

    private async Task RemoveUserModifyRecords(string userName, Source s1, Source s2)
    {
        var source1Id = await _clientIdentityRepository.GetIdBySource(s1.Name, s1.Id);
        var source2Id = await _clientIdentityRepository.GetIdBySource(s2.Name, s2.Id);

        if (source1Id != null)
            await _userModifyRecordsService.RemoveModify(userName, source1Id ?? 0);

        if (source2Id != null)
            await _userModifyRecordsService.RemoveModify(userName, source2Id ?? 0);
    }

}