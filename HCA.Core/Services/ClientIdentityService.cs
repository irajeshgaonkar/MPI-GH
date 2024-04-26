using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.Sqs;
using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.Logging;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Request;
using HCA.Models.MuleSoft.Response;
using HCA.Models.Request;
using HCA.Models.Request.DOH;
using HCA.Models.Response;
using HCA.Models.SQS;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
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

    private readonly IAppLogger _logger;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientIdentityService(IUserRequestRepository userRequestRepository,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor,
        IClientIdentityRepository clientIdentityRepository,
        IUserModifyRecordsRepository userModifyRecordsRepository,
        IRequestProcessLogRepository requestProcessLogRepository,
        IUserModifyRecordsService userModifyRecordsService,
        ISqsPublisher sqsPublisher, IUserRequestMapper userRequestMapper,
        IAppLogger logger, 
        IHttpContextAccessor httpContextAccessor)
    {
        _userRequestRepository = userRequestRepository;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
        _clientIdentityRepository = clientIdentityRepository;
        _userModifyRecordsRepository = userModifyRecordsRepository;
        _requestProcessLogRepository = requestProcessLogRepository;
        _sqsPublisher = sqsPublisher;
        _userRequestMapper = userRequestMapper;
        _userModifyRecordsService = userModifyRecordsService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(int, IEnumerable<ClientIdentityModel>)> GetAll( string currentUser, Dictionary<string, string> searchFilter, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "" )
    {
        //var userModifyRecords = (await _userModifyRecordsRepository.GetAllAsync(r => r.UserName == currentUser)).Select(t => t.ClientIdentityId).ToList();
        var userModifyRecords = new List<int>();
        var (count, entities) = await _clientIdentityRepository.GetAll( searchFilter, userModifyRecords, pageNumber, recordsPerPage, orderBy );
        var models = ClientIdentityMapper.MapToClientIdentityModel(entities);
        return (count, models);
    }

    public async Task<dynamic?> PostIdentities( IEnumerable<ClientIdentityRequest> identities, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        var trackingId = $"{ApiCallType.VEPost.GetStringValue()}-{identities.First().SourceSystemName}-{identities.First().SourceSystemName}";
        var userRequestEntity = CreateUserRequest(identities, ApiCallType.VELink, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VELink, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await PostIdentities( userRequestEntity, identities );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DOH_PostIdentities( DOH_PostClientIdentityRequest request, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new UserRequestEntity();
        try
        {
            string strIdentities = request.Content.Identity.ToString();
            Identity? identity = JsonConvert.DeserializeObject<Identity>(strIdentities);

            if (!string.Equals(request.SourceSystem, identity.Sources[0].Name, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorResponseBuilder(request.TrackingId, "Source system mismatch.");
            }

            string trackingId = request.TrackingId
                                ?? ClientIdentityRequestExtension.GetTrackingId(identity, ApiCallType.DOH_VEPost);

            DOH_PostClientIdentityRequest dOH_PostClientIdentityRequest = new(trackingId)
            {
                SourceSystem = request.SourceSystem,
                Agency = request.Agency,
                protectedPopulation = request.protectedPopulation
            };
            if (strIdentities.ToLower().Contains("null"))
            {
                // Replace null values with empty strings and get modified JSON string 
                dynamic modifiedJson = ReplaceNullValues(request.Content.Identity.ToString());

                JsonElement modifiedJsonElement = ConvertJObjectToJsonElement(modifiedJson);

                DOH_PostIdentityRequestContent postIdentityRequestContent = new(modifiedJsonElement)
                {
                    ResponseIdentityFormatNames = request.Content.ResponseIdentityFormatNames
                };
                dOH_PostClientIdentityRequest.Content = postIdentityRequestContent;
            }
            else
            {
                dOH_PostClientIdentityRequest.Content = request.Content;
            }


            userRequestEntity = CreateUserRequest(dOH_PostClientIdentityRequest, ApiCallType.DOH_VEPost, currentUser, trackingId, notificationOptions);
            
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VELink, userRequestEntity);
                return trackingId;
            }

            return await DOH_PostIdentities(userRequestEntity, dOH_PostClientIdentityRequest);
        }
        catch ( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = request.Agency,
                Role = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value,
                FunctionName = nameof(DOH_DemographicQuery),
                ErrorMessage = e.Message,
                ErrorCode = (e.InnerException as WebException)?.Response is HttpWebResponse httpReponse ? httpReponse.StatusCode.ToString() : null
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DemographicQuery)}-Failed",
                TrackingId = request.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = request.Agency,
                Role = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value,
                FunctionName = nameof(DOH_DemographicQuery),
                ErrorMessage = e.Message,
                ErrorCode = (e.InnerException as WebException)?.Response is HttpWebResponse httpReponse ? httpReponse.StatusCode.ToString() : null
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DemographicQuery)}-Failed",
                TrackingId = request.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
    }



    private JsonElement? ConvertJObjectToJsonElement( JObject jObject )
    {
        if( jObject == null )
        {
            return null;
        }
        // Serialize the JObject to a JSON string
        string jsonString = jObject.ToString();

        // Parse the JSON string into a JsonDocument
        using( JsonDocument document = JsonDocument.Parse( jsonString ) )
        {
            // Get the root element of the JsonDocument
            JsonElement rootElement = document.RootElement;

            // Return the root element
            return rootElement.Clone();
        }
    }

    public JObject ReplaceNullValues( string jsonString )
    {
        JObject jsonObject = JObject.Parse(jsonString);
        ReplaceNullValues( jsonObject );
        return jsonObject;
    }

    private void ReplaceNullValues( JObject obj )
    {
        foreach( var property in obj.Properties() )
        {
            if( property.Value.Type == JTokenType.Null )
            {
                property.Value = "";
            }
            else if( property.Value.Type == JTokenType.Object )
            {
                ReplaceNullValues( (JObject)property.Value );
            }
            else if( property.Value.Type == JTokenType.Array )
            {
                JArray array = (JArray)property.Value;
                for( int i = 0; i < array.Count; i++ )
                {
                    if( array[i].Type == JTokenType.Null )
                    {
                        array[i] = "";
                    }
                    else if( array[i].Type == JTokenType.Object )
                    {
                        ReplaceNullValues( (JObject)array[i] );
                    }
                }
            }

        }
    }


    public async Task<dynamic?> LinkIdentities( LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        var trackingId = $"{ApiCallType.VEUnLink.GetStringValue()}-{linkingSources.Source.GetTrackingId(linkingSources.LinkToSource)}";
        var userRequestEntity = CreateUserRequest(linkingSources, ApiCallType.VELink, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VELink, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await LinkIdentities( userRequestEntity, linkingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> UnLinkIdentities( UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        var trackingId = $"{ApiCallType.VEUnLink.GetStringValue()}-{unLinkingSources.Source.GetTrackingId(unLinkingSources.UnlinkFromSource)}";
        var userRequestEntity = CreateUserRequest(unLinkingSources, ApiCallType.VEUnLink, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VEUnLink, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unLinkingSources.UnlinkFromSource, unLinkingSources.Source);
            return await UnLinkIdentities( userRequestEntity, unLinkingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> MergeIdentities( MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        var trackingId = $"{ApiCallType.VEMerge.GetStringValue()}-{mergingSources.ToSurviveSource.GetTrackingId(mergingSources.ToRetireSource)}";
        var userRequestEntity = CreateUserRequest(mergingSources, ApiCallType.VEMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VEMerge, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);
            return await MergeIdentities( userRequestEntity, mergingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> UnMergeIdentities( UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        var trackingId = $"{ApiCallType.VEUnMerge.GetStringValue()}-{unMergingSources.UnmergeSource.GetTrackingId(unMergingSources.UnmergeSource)}";
        var userRequestEntity = CreateUserRequest(unMergingSources, ApiCallType.VEUnMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VEUnMerge, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unMergingSources.UnmergeSource, unMergingSources.UnmergeFromSource);
            return await UnMergeIdentities( userRequestEntity, unMergingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DemographicSearch( Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        var trackingId = $"{ApiCallType.VEDemographicSearch.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
        var userRequestEntity = CreateUserRequest(filter, ApiCallType.VEDemographicSearch, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VEDemographicSearch, userRequestEntity );
                return trackingId;
            }

            return await DemographicSearch( userRequestEntity, filter );
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Success, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DemographicQuery( Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions ) //,string responseIdentityFormatNames = "DEFAULT")
    {
        var trackingId = $"{ApiCallType.VEDemographicQuery.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
        var userRequestEntity = CreateUserRequest(filter, ApiCallType.VEDemographicQuery, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VEDemographicQuery, userRequestEntity );
                return trackingId;
            }

            return await DemographicQuery( userRequestEntity, filter );
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Success, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DOH_DemographicSearch( DOH_DemographicsSearchRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        // TODO: empty string init not required; logic can be simplified (also extracted to shared logic)
        var trackingId = string.Empty;
        if( !(string.IsNullOrEmpty( filter.Trackingid)) && (filter.Trackingid.Length >= 1) )
        {
            trackingId = filter.Trackingid.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.VEDemographicSearch.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
        }
        if( filter.content.responseIdentityFormatNames == null || (filter.content.responseIdentityFormatNames != null && filter.content.responseIdentityFormatNames[0] == "") )
        {
            filter.content.responseIdentityFormatNames = new string[] { "DEFAULT" };
        }

        string strIdentities = filter.content.identity.ToString();

        DOH_DemographicsSearchRequest dOH_DemographicQueryRequest = new DOH_DemographicsSearchRequest();

        // TODO: dedup internal logic, and look into deduping global logic
        dOH_DemographicQueryRequest.SourceSystem = filter.SourceSystem;
        dOH_DemographicQueryRequest.Agency = filter.Agency;
        if( strIdentities.ToLower().Contains( "null" ) )
        {
            // Replace null values with empty strings and get modified JSON string 
            dynamic modifiedJson = ReplaceNullValues(filter.content.identity.ToString());

            JsonElement modifiedJsonElement = ConvertJObjectToJsonElement(modifiedJson);

            ContentSearch content = new ContentSearch();
            content.identity = modifiedJsonElement;
            content.responseIdentityFormatNames = filter.content.responseIdentityFormatNames;
            content.matchScoreThreshold = filter.content.matchScoreThreshold;
            content.maxSearchResults = filter.content.maxSearchResults;

            dOH_DemographicQueryRequest.content = content;
        }
        else
        {
            dOH_DemographicQueryRequest.content = filter.content;
        }

        var userRequestEntity = CreateUserRequest(dOH_DemographicQueryRequest, ApiCallType.VEDemographicSearch, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.VEDemographicSearch, userRequestEntity );
                return trackingId;
            }

            return await DOH_DemographicSearch( userRequestEntity, dOH_DemographicQueryRequest );
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Success, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DOH_DemographicQuery( DOH_DemographicQueryRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions ) //,string responseIdentityFormatNames = "DEFAULT")
    {
        UserRequestEntity userRequestEntity = new UserRequestEntity();

        try
        {            
            var trackingId = string.Empty;
            if (!(string.IsNullOrEmpty(filter.Trackingid)) && (filter.Trackingid.Length >= 1))
            {
                trackingId = filter.Trackingid.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.DOH_VEDemographicQuery.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";

            }
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
                dOH_DemographicQueryRequest.SourceSystem = filter.SourceSystem;
                dOH_DemographicQueryRequest.Agency = filter.Agency;
            }
            else
            {
                dOH_DemographicQueryRequest.content = filter.content;
                dOH_DemographicQueryRequest.SourceSystem = filter.SourceSystem;
                dOH_DemographicQueryRequest.Agency = filter.Agency;
            }


            userRequestEntity = CreateUserRequest(dOH_DemographicQueryRequest, ApiCallType.DOH_VEDemographicQuery, currentUser, trackingId, notificationOptions);


            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEDemographicQuery, userRequestEntity);
                return trackingId;
            }

            return await DOH_DemographicQuery(userRequestEntity, dOH_DemographicQueryRequest);
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = filter.Agency,
                Role = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value,
                FunctionName = nameof(DOH_DemographicQuery),
                ErrorMessage = e.Message,
                ErrorCode = (e.InnerException as WebException)?.Response is HttpWebResponse httpReponse ? httpReponse.StatusCode.ToString() : null
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DemographicQuery)}-Failed",
                TrackingId = filter.Trackingid,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }
    }

    public async Task<dynamic?> DOH_LinkIdentities( DOH_LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        if (!string.Equals(linkingSources.content.LinkToSource.Name, linkingSources.content.Source.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponseBuilder(linkingSources.TrackingId, "LinkToSource and Source do not match.");
        }
        if (!string.Equals(linkingSources.SourceSystem, linkingSources.content.LinkToSource.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponseBuilder(linkingSources.TrackingId, "Calling SourceSystem and LinkToSource do not match.");
        }
        if (!string.Equals(linkingSources.SourceSystem, linkingSources.content.Source.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponseBuilder(linkingSources.TrackingId, "Calling SourceSystem and Source do not match.");
        }
        var trackingId = string.Empty;
        if( !(string.IsNullOrEmpty( linkingSources.TrackingId)) && (linkingSources.TrackingId.Length >= 1) )
        {
            trackingId = linkingSources.TrackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VELink.GetStringValue()}-{linkingSources.content.Source.GetTrackingId( linkingSources.content.LinkToSource )}";
        }
        var userRequestEntity = CreateUserRequest(linkingSources, ApiCallType.DOH_VELink, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.DOH_VELink, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await DOH_LinkIdentities( userRequestEntity, linkingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DOH_UnLinkIdentities( DOH_UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        if (!string.Equals(unLinkingSources.content.UnlinkFromSource.Name, unLinkingSources.content.Source.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponseBuilder(unLinkingSources.TrackingId, "UnlinkFromSource and Source do not match.");
        }
        if (!string.Equals(unLinkingSources.SourceSystem, unLinkingSources.content.UnlinkFromSource.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponseBuilder(unLinkingSources.TrackingId, "SourceSystem and UnlinkFromSource do not match.");
        }
        if (!string.Equals(unLinkingSources.SourceSystem, unLinkingSources.content.Source.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponseBuilder(unLinkingSources.TrackingId, "SourceSystem and Source do not match.");
        }
        var trackingId = string.Empty;
        if( !(string.IsNullOrEmpty( unLinkingSources.TrackingId)) && (unLinkingSources.TrackingId.Length >= 1) )
        {
            trackingId = unLinkingSources.TrackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VEUnLink.GetStringValue()}-{unLinkingSources.content.Source.GetTrackingId( unLinkingSources.content.UnlinkFromSource )}";
        }
        var userRequestEntity = CreateUserRequest(unLinkingSources, ApiCallType.DOH_VEUnLink, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.DOH_VEUnLink, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unLinkingSources.UnlinkFromSource, unLinkingSources.Source);
            return await DOH_UnLinkIdentities( userRequestEntity, unLinkingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DOH_MergeIdentities( DOH_MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        if( !string.Equals( mergingSources.content.ToSurviveSource.Name, mergingSources.content.ToRetireSource.Name, StringComparison.OrdinalIgnoreCase ) )
        {
            return ErrorResponseBuilder( mergingSources.TrackingId, "ToSurviveSource and ToRetireSource do not match." );
        }
        if( !string.Equals( mergingSources.SourceSystem, mergingSources.content.ToSurviveSource.Name, StringComparison.OrdinalIgnoreCase ))
        {
            return ErrorResponseBuilder( mergingSources.TrackingId, "SourceSystem and ToSurviveSource do not match." );
        }
        if ( !string.Equals( mergingSources.SourceSystem, mergingSources.content.ToRetireSource.Name, StringComparison.OrdinalIgnoreCase ))
        {
            return ErrorResponseBuilder( mergingSources.TrackingId, "SourceSystem and ToRetireSource do not match." );
        }

        var trackingId = string.Empty;
        if( !(string.IsNullOrEmpty( mergingSources.TrackingId)) && (mergingSources.TrackingId.Length >= 1) )
        {
            trackingId = mergingSources.TrackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VEMerge.GetStringValue()}-{mergingSources.content.ToSurviveSource.GetTrackingId( mergingSources.content.ToRetireSource )}";
        }
        var userRequestEntity = CreateUserRequest(mergingSources, ApiCallType.DOH_VEMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.DOH_VEMerge, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);
            return await DOH_MergeIdentities( userRequestEntity, mergingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    private static object ErrorResponseBuilder( string trackingId, string errorMessage ) 
        => new { errorCode = "400", message = errorMessage, success = false, trackingId = trackingId ?? "" };

    public async Task<dynamic?> DOH_UnMergeIdentities( DOH_UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        if ( !string.Equals( unMergingSources.content.UnmergeFromSource.Name, unMergingSources.content.UnmergeSource.Name, StringComparison.OrdinalIgnoreCase ))
        {
            return ErrorResponseBuilder( unMergingSources.TrackingId, "UnmergeFromSource and UnmergeSource do not match." );
        }
        if ( !string.Equals( unMergingSources.SourceSystem, unMergingSources.content.UnmergeFromSource.Name, StringComparison.OrdinalIgnoreCase ))
        {
            return ErrorResponseBuilder( unMergingSources.TrackingId, "SourceSystem and UnmergeFromSource do not match." );
        }
        if ( !string.Equals( unMergingSources.SourceSystem, unMergingSources.content.UnmergeSource.Name, StringComparison.OrdinalIgnoreCase ))
        {
            return ErrorResponseBuilder( unMergingSources.TrackingId, "SourceSystem and UnmergeSource do not match." );
        }
        var trackingId = string.Empty;
        if( !(string.IsNullOrEmpty( unMergingSources.TrackingId)) && (unMergingSources.TrackingId.Length >= 1) )
        {
            trackingId = unMergingSources.TrackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VEUnMerge.GetStringValue()}-{unMergingSources.content.UnmergeSource.GetTrackingId( unMergingSources.content.UnmergeSource )}";
        }

        var userRequestEntity = CreateUserRequest(unMergingSources, ApiCallType.DOH_VEUnMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.DOH_VEUnMerge, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unMergingSources.UnmergeSource, unMergingSources.UnmergeFromSource);
            return await DOH_UnMergeIdentities( userRequestEntity, unMergingSources );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    public async Task<dynamic?> DOH_DeleteSourceIdentity( DOH_DeleteClientIdentityRequest deleteSourceIdentity, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        if (!string.Equals(deleteSourceIdentity.SourceSystem, deleteSourceIdentity.Content.Source.Name, StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponseBuilder(deleteSourceIdentity.TrackingId, "SourceSystem and Source do not match.");
        }
        var trackingId = string.Empty;
        if( !(string.IsNullOrEmpty( deleteSourceIdentity.TrackingId )) && (deleteSourceIdentity.TrackingId.Length >= 1) )
        {
            trackingId = deleteSourceIdentity.TrackingId.ToString();
        }
        else
        {
            trackingId = $"{ApiCallType.DOH_VEDelete.GetStringValue()}-{deleteSourceIdentity.Content.Source.GetTrackingId( deleteSourceIdentity.Content.Source )}";
        }
        var userRequestEntity = CreateUserRequest(deleteSourceIdentity, ApiCallType.DOH_VEDelete, currentUser, trackingId, notificationOptions);

        try
        {
            if( processType == ProcessType.Async )
            {
                await PublishMessageToSqs( ApiCallType.DOH_VEDelete, userRequestEntity );
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);

            return await DOH_DeleteSourceIdentity( userRequestEntity, deleteSourceIdentity );
        }
        catch( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            throw;
        }
        catch( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.ToString() );
            throw;
        }
    }

    private async Task<PostIdentityResponseContent?> PostIdentities( UserRequestEntity userRequestEntity, IEnumerable<ClientIdentityRequest> identities )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new PostClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = identities.ToList()
        };

        var response = await _clientIdentityRequestExecutor.Execute<PostClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response?.Content;
    }

    private async Task<dynamic?> DOH_PostIdentities( UserRequestEntity userRequestEntity, DOH_PostClientIdentityRequest request )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        if( request.Content.ResponseIdentityFormatNames == null || (request.Content.ResponseIdentityFormatNames != null && request.Content.ResponseIdentityFormatNames[0] == "") )
        {
            request.Content.ResponseIdentityFormatNames = new string[] { "DEFAULT" };
        }

        var linkIdentityRequest = new DOH_PostClientIdentityRequest(userRequestEntity.TrackingId)
        {
            protectedPopulation = request.protectedPopulation,
            SourceSystem = request.SourceSystem,
            Agency = request.Agency,
            Content = request.Content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_PostClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        FilterPostResponse(request, response);
        return response;
    }

    private void FilterPostResponse( DOH_PostClientIdentityRequest request, DOH_PostClientIdentityResponse? response )
    {
        string responseContent = Convert.ToString(response.Content);

        if (string.IsNullOrEmpty(responseContent))
        {
            response.Content = null;
            return;
        }

        JObject jsonObjectResponse = JObject.Parse(responseContent);

        JArray newArray = new();

        if( request.Content.ResponseIdentityFormatNames[0].ToString().ToUpper() == "GROUP_BY_SOURCE" )
        {
            JArray? identityGroupedBySource = jsonObjectResponse["identityGroupedBySource"] as JArray;
            JArray sources = new();
            // TODO: null checks/validation/strong typing
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach( var source in identityGroupedBySource )
            {
                if( source["source"]["name"].ToString().ToLower() == request.SourceSystem.ToLower() )
                {
                    sources.Add( source );
                }
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if( sources != null && sources.Count > 0 )
            {
                jsonObjectResponse["identityGroupedBySource"] = sources;
                newArray.Add( jsonObjectResponse );
            }
        }
        else
        {
            // TODO: null checks/validation/strong typing
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            JArray? SourceArray = jsonObjectResponse["linkIdentity"]["sources"] as JArray;
            JArray sources = new();
            foreach( var source in SourceArray )
            {
                if( source["name"].ToString().ToLower() == request.SourceSystem.ToLower() )
                {
                    sources.Add( source );
                }
            }
            if( sources != null && sources.Count > 0 )
            {
                jsonObjectResponse["linkIdentity"]["sources"] = sources;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                newArray.Add( jsonObjectResponse );
            }
        }
        response.Content = ConvertJObjectToJsonElement( jsonObjectResponse );
    }

    private async Task<LinkIdentitiesResponseContent?> LinkIdentities( UserRequestEntity userRequestEntity, LinkingSources linkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new LinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = linkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<LinkClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response?.Content;
    }

    private async Task<List<PostIdentityResponseContent>?> DemographicSearch( UserRequestEntity userRequestEntity, Identity filter )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicSearchClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicSearchClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response?.Content;
    }

    private async Task<dynamic?> DOH_DemographicSearch( UserRequestEntity userRequestEntity, DOH_DemographicsSearchRequest filter )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DOH_DemographicSearchClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = filter.SourceSystem,
            Agency = filter.Agency,
            Content = filter.content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_DemographicSearchClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );

        FilterSearchResponse(filter, response);

        return response;
    }

    private void FilterSearchResponse( DOH_DemographicsSearchRequest filter, DOH_DemographicSearchClientIdentityResponse response )
    {
        JObject jsonObject = JObject.Parse(response.Content.ToString());

        // TODO: refactor to strongly-typed classes
        JArray searchResults = (JArray)jsonObject["searchResults"];
        if( searchResults == null ) { return; }

        JArray filteredResults = new();

        if( filter.content.responseIdentityFormatNames[0].ToString().ToUpper() == "GROUP_BY_SOURCE" )
        {
            foreach( var item in searchResults )
            {
                FilterSearchResultsGroupBySource( filter, filteredResults, item );
            }
        }
        else
        {
            foreach( var item in searchResults )
            {
                FilterSearchResultsDefault( filter, filteredResults, item );
            }
        }

        jsonObject["searchResults"] = filteredResults;
        response.Content = ConvertJObjectToJsonElement( jsonObject );
    }

    // TODO: check if shared logic can be extracted
    private static void FilterSearchResultsDefault( DOH_DemographicsSearchRequest filter, JArray filteredResults, JToken item )
    {
        JArray identityGroupedBySourceArray = (JArray)item["identity"]["sources"];
        JArray sources = new();
        foreach( var source in identityGroupedBySourceArray )
        {
            if( source["name"].ToString().ToLower() == filter.SourceSystem.ToLower() )
            {
                sources.Add( source );
            }
        }
        if( sources != null && sources.Count > 0 )
        {
            item["identity"]["sources"] = sources;
            filteredResults.Add( item );
        }
    }

    private static void FilterSearchResultsGroupBySource( DOH_DemographicsSearchRequest filter, JArray newArray, JToken item )
    {
        JArray identityGroupedBySourceArray = (JArray)item["identityGroupedBySource"];
        JArray sources = new();
        foreach( var source in identityGroupedBySourceArray )
        {
            if( source["source"]["name"].ToString().ToLower() == filter.SourceSystem.ToLower() )
            {
                sources.Add( source );
            }
        }
        if( sources != null && sources.Count > 0 )
        {
            item["identityGroupedBySource"] = sources;
            newArray.Add( item );
        }
    }

    private async Task<DemographicQueryResponseContent?> DemographicQuery( UserRequestEntity userRequestEntity, Identity filter )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicQueryClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response?.Content;
    }

    // TODO: use inheritance to dedup filter/sourceSystem logic
    private async Task<dynamic?> DOH_DemographicQuery( UserRequestEntity userRequestEntity, DOH_DemographicQueryRequest filter )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DOH_DemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = filter.SourceSystem,
            Agency = filter.Agency,
            Content = filter.content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_DemographicQueryClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );

        FilterQueryResponse(filter, response);    

        return response;
    }

    private void FilterQueryResponse( DOH_DemographicQueryRequest filter, DOH_DemographicQueryClientIdentityResponse? response )
    {
        JObject jsonObject = JObject.Parse(response.Content.ToString());
        JArray identityGroupedBySource = (JArray)jsonObject["identityGroupedBySource"];

        JArray newArray = new();

        if(jsonObject !=null && jsonObject.Count > 0 )
        {
            if (filter.content.responseIdentityFormatNames[0].ToString().ToUpper() == "GROUP_BY_SOURCE")
            {
                JArray sources = new();
                foreach (var source in identityGroupedBySource)
                {
                    if (source["source"]["name"].ToString().ToLower() == filter.SourceSystem.ToLower())
                    {
                        sources.Add(source);
                    }
                }
                if (sources != null && sources.Count > 0)
                {
                    jsonObject["identityGroupedBySource"] = sources;
                    newArray.Add(jsonObject);
                }
                else
                {
                    jsonObject = null;
                    newArray.Add(jsonObject);

                }


            }
            else
            {
                JArray SourceArray = (JArray)jsonObject["identity"]["sources"];
                JArray sources = new();
                foreach (var source in SourceArray)
                {
                    if (source["name"].ToString().ToLower() == filter.SourceSystem.ToLower())
                    {
                        sources.Add(source);
                    }
                }
                if (sources != null && sources.Count > 0)
                {
                    jsonObject["identity"]["sources"] = sources;
                    newArray.Add(jsonObject);
                }
                else
                {
                    jsonObject = null;
                    newArray.Add(jsonObject);

                }
            }
        }
        

        response.Content = ConvertJObjectToJsonElement( jsonObject );
    }

    private async Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities( UserRequestEntity userRequestEntity, UnLinkingSources unLinkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var unLinkClientIdentityRequest = new UnLinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unLinkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<UnLinkClientIdentityResponse>(unLinkClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response?.Content;
    }

    private async Task<MergeIdentitiesResponseContent?> MergeIdentities( UserRequestEntity userRequestEntity, MergingSources mergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var mergeClientIdentityRequest = new MergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = mergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<MergeClientIdentityResponse>(mergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response?.Content;
    }

    private async Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities( UserRequestEntity userRequestEntity, UnMergingSources unMergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var unMergeClientIdentityRequest = new UnMergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unMergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<UnMergeClientIdentityResponse>(unMergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response?.Content;
    }

    private async Task<dynamic?> DOH_LinkIdentities( UserRequestEntity userRequestEntity, DOH_LinkingSources linkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new DOH_LinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = linkingSources.SourceSystem,
            Agency = linkingSources.Agency,
            Content = linkingSources.content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_LinkClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response;
    }

    private async Task<dynamic?> DOH_UnLinkIdentities( UserRequestEntity userRequestEntity, DOH_UnLinkingSources unLinkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var unLinkClientIdentityRequest = new DOH_UnLinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = unLinkingSources.SourceSystem,
            Agency = unLinkingSources.Agency,
            Content = unLinkingSources.content
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_UnLinkClientIdentityResponse>(unLinkClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response;
    }

    private async Task<dynamic?> DOH_MergeIdentities( UserRequestEntity userRequestEntity, DOH_MergingSources mergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var mergeClientIdentityRequest = new DOH_MergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = mergingSources.SourceSystem,
            Agency = mergingSources.Agency,
            Content = mergingSources.content
        };
        var response = await _clientIdentityRequestExecutor.Execute<DOH_MergeClientIdentityResponse>(mergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response;
    }

    private async Task<dynamic?> DOH_DeleteSourceIdentity( UserRequestEntity userRequestEntity, DOH_DeleteClientIdentityRequest deleteSourceIdentity )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var deleteClientIdentityRequest = new DOH_DeleteClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = deleteSourceIdentity.SourceSystem,
            Agency = deleteSourceIdentity.Agency,
            Content = deleteSourceIdentity.Content
        };
        var response = await _clientIdentityRequestExecutor.Execute<DOH_DeleteClientIdentityResponse>(deleteClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response;
    }

    private async Task<dynamic?> DOH_UnMergeIdentities( UserRequestEntity userRequestEntity, DOH_UnMergingSources unMergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var unMergeClientIdentityRequest = new DOH_UnMergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = unMergingSources.SourceSystem,
            Agency = unMergingSources.Agency,
            Content = unMergingSources.content
        };
        var response = await _clientIdentityRequestExecutor.Execute<DOH_UnMergeClientIdentityResponse>(unMergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus( userRequestEntity, RequestStatus.Success, "Request Processed Successfully" );
        return response;
    }

    private async Task PublishMessageToSqs( ApiCallType apiCallType, UserRequestEntity userRequestEntity )
    {
        var messageType = MessageType.UserRequest;
        if( messageType == null ) throw new ArgumentException( $"ClientIdentityService:PublishMessageToSqs: cannot process message for {apiCallType.GetStringValue()}" );

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

        await _sqsPublisher.PublishMessage( sqsMessage );
    }

    private UserRequestEntity CreateUserRequest<T>( T request, ApiCallType apiCallType, string userName, string trackingId, NotificationOptions? notificationOptions )
    {
        if( request == null ) throw new ArgumentNullException( "ClientIdentityService:CreateUserRequest:Request cannot be null" );

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

        _userRequestRepository.AddAsync( userRequest );
        return userRequest;
    }

    private void UpdateProcessStatus( UserRequestEntity userRequest, RequestStatus status, string message )
    {
        userRequest.Status = status.GetStringValue();
        userRequest.Message = message;
        userRequest.ProcessEndTime = DateTime.Now;
        _userRequestRepository.Update( userRequest );
    }

    private async Task RemoveUserModifyRecords( string userName, Source s1, Source s2 )
    {
        var source1Id = await _clientIdentityRepository.GetIdBySource(s1.Name, s1.Id);
        var source2Id = await _clientIdentityRepository.GetIdBySource(s2.Name, s2.Id);

        if( source1Id != null )
            await _userModifyRecordsService.RemoveModify( userName, source1Id ?? 0 );

        if( source2Id != null )
            await _userModifyRecordsService.RemoveModify( userName, source2Id ?? 0 );
    }

}