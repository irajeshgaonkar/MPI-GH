using System.Security.Claims;
using System.Text.Json;
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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HCA.Core.Services;

public class ClientIdentityService : IClientIdentityService
{
    private readonly IUserRequestRepository _userRequestRepository;
    private readonly IUserModifyRecordsService _userModifyRecordsService;
    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;
    private readonly IClientIdentityRepository _clientIdentityRepository;
    private readonly IUserModifyRecordsRepository _userModifyRecordsRepository;
    private readonly IRequestProcessLogRepository _requestProcessLogRepository;
    private static IAppRoleMappingRepository _appRoleMappingRepository;
    private readonly IDataShareMappingRepository _dataShareMappingRepository;
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
        IAppRoleMappingRepository appRoleMappingRepository,
        IDataShareMappingRepository dataShareMappingRepository,
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
        _appRoleMappingRepository = appRoleMappingRepository;
        _dataShareMappingRepository = dataShareMappingRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(int, IEnumerable<ClientIdentityModel>)> GetAll( string currentUser, Dictionary<string, string> searchFilter, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "" )
    {
        try
        {
            var userModifyRecords = new List<int>();

            var sourceSystemScopes = await GetSourceSysteAccessFromContext(_httpContextAccessor.HttpContext);
            if (!sourceSystemScopes.Item1)
            {
                searchFilter.Add("SourceSystemNames", string.Join(',', sourceSystemScopes.Item2));
            }

            var (count, entities) = await _clientIdentityRepository.GetAll(searchFilter, userModifyRecords, pageNumber, recordsPerPage, orderBy);
            var models = ClientIdentityMapper.MapToClientIdentityModel(entities);
            return (count, models);
        }
        catch(Exception ex)
        {
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(GetAll),
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(GetAll)}-Failed",
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(ex, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
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
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(PostIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(PostIdentities)}-Failed",
                TrackingId = trackingId,
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
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(PostIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(PostIdentities)}-Failed",
                TrackingId= trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(PostIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(PostIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
    }

    public async Task<dynamic?> DOH_PostIdentities( DOH_PostClientIdentityRequest request, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new();
        try
        {
            string strIdentities = request.Content.Identity.ToString();
            Identity? identity = JsonConvert.DeserializeObject<Identity>(strIdentities);
            if( ValidateIdentity( request, identity ) is object errorResponse )
            {
                return errorResponse;
            }

            string trackingId = string.IsNullOrWhiteSpace(request.TrackingId)
                                ? ClientIdentityRequestExtension.GetTrackingId(identity, ApiCallType.DOH_VEPost): request.TrackingId;

            if (request.Content.ResponseIdentityFormatNames == null || request.Content.ResponseIdentityFormatNames.Length == 0
                || request.Content.ResponseIdentityFormatNames.Any(view => string.IsNullOrWhiteSpace(view)))
            {
                request.Content.ResponseIdentityFormatNames = ["DEFAULT"];
            }

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
        catch (Exception e)
        {
            if(userRequestEntity.Id > 0)
            {
                UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            }

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = request.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_PostIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(DOH_PostIdentities)}-Failed",
                TrackingId = request.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
    }

    private static object? ValidateIdentity( DOH_PostClientIdentityRequest request, [System.Diagnostics.CodeAnalysis.NotNull] Identity? identity )
    {
        //If there are no sources in Identity request
        //Or Sources are missing Name, we will not be able to match the source system
        if( identity == null )
        {
            identity = new Identity();
            return ErrorResponseBuilder( request.TrackingId, "Identity can not be Null or Empty" );
        }
        if( identity.Sources.Count == 0 )
        {
            return ErrorResponseBuilder( request.TrackingId, "Identity Sources can not be Null or Empty" );
        }
        if( identity.Sources.Any( s => string.IsNullOrWhiteSpace( s.Name ) ) )
        {
            return ErrorResponseBuilder( request.TrackingId, "Identity Source Name can not be Null or Empty" );
        }

        if( !string.Equals( request.SourceSystem, identity.Sources[0]?.Name, StringComparison.OrdinalIgnoreCase ) )
        {
            if( !string.Equals( request.SourceSystem, identity.Sources[0].Name.Split( '.' )[0], StringComparison.OrdinalIgnoreCase ) )
            {
                return ErrorResponseBuilder( request.TrackingId, "Source system mismatch." );
            }
        }

        return null;
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
        var trackingId = $"{ApiCallType.VELink.GetStringValue()}-{linkingSources.Source.GetTrackingId(linkingSources.LinkToSource)}";
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

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(LinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(LinkIdentities)}-Failed",
                TrackingId = trackingId,
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
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(LinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(LinkIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(LinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(LinkIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
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

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(UnLinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(UnLinkIdentities)}-Failed",
                TrackingId = trackingId,
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
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(UnLinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(UnLinkIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(UnLinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(UnLinkIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
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

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(MergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(MergeIdentities)}-Failed",
                TrackingId = trackingId,
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
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(MergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(MergeIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(MergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(MergeIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
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

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(UnMergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(UnMergeIdentities)}-Failed",
                TrackingId = trackingId,
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
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(UnMergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(UnMergeIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(UnMergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(UnMergeIdentities)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
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

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DemographicSearch),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(DemographicSearch)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DemographicSearch),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(DemographicSearch)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
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

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DemographicQuery),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(DemographicQuery)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DemographicQuery),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Constants.LogPrefix_API}-{nameof(DemographicQuery)}-Failed",
                TrackingId = trackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
    }

    public async Task<dynamic?> DOH_DemographicSearch( DOH_DemographicsSearchRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new();
        // TODO: empty string init not required; logic can be simplified (also extracted to shared logic)
        var trackingId = string.Empty;
        try
        {
            if (!(string.IsNullOrEmpty(filter.Trackingid)) && (filter.Trackingid.Length >= 1))
            {
                trackingId = filter.Trackingid.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.VEDemographicSearch.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
            }

            if (filter.content.responseIdentityFormatNames == null || filter.content.responseIdentityFormatNames.Length == 0
                || filter.content.responseIdentityFormatNames.Any(view => string.IsNullOrWhiteSpace(view)))
            {
                filter.content.responseIdentityFormatNames = ["DEFAULT"];
            }

            string strIdentities = filter.content.identity.ToString();

            DOH_DemographicsSearchRequest dOH_DemographicQueryRequest = new DOH_DemographicsSearchRequest();

            // TODO: dedup internal logic, and look into deduping global logic
            dOH_DemographicQueryRequest.SourceSystem = filter.SourceSystem;
            dOH_DemographicQueryRequest.Agency = filter.Agency;
            if (strIdentities.ToLower().Contains("null"))
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

            userRequestEntity = CreateUserRequest(dOH_DemographicQueryRequest, ApiCallType.VEDemographicSearch, currentUser, trackingId, notificationOptions);
            
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEDemographicSearch, userRequestEntity);
                return trackingId;
            }

            return await DOH_DemographicSearch(userRequestEntity, dOH_DemographicQueryRequest);
        }
        catch ( HcaMuleSoftException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Success, e.ToString() );
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = filter.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_DemographicSearch),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DemographicSearch)}-Failed",
                TrackingId = filter.Trackingid,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = filter.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_DemographicSearch),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DemographicSearch)}-Failed",
                TrackingId = filter.Trackingid,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }

    }

    public async Task<IdentityExistsResponse?> IdentityExistsAsync(IdentityExistsRequest request, string currentUser, NotificationOptions? notificationOptions)
    {
        UserRequestEntity userRequestEntity = new();
        DOH_DemographicQueryRequest demographicsQueryRequest = new();
        var trackingId = string.Empty;

        try
        {
            if( string.IsNullOrEmpty( request.TrackingId ) || request.TrackingId.Length < 1 )
            {
                trackingId = $"{ApiCallType.VEIdentityExists.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
            }
            else 
            {
                trackingId = request.TrackingId.ToString();
            }

            if(string.IsNullOrEmpty(request.SourceSystem))
            {
                throw new HcaBadRequestException("SourceSystem is required.");
            }

            string strIdentities = request.Content.Identity.ToString();

            if (strIdentities.Contains("null", StringComparison.CurrentCultureIgnoreCase))
            {
                // Replace null values with empty strings and get modified JSON string
                dynamic modifiedJson = ReplaceNullValues(request.Content.Identity.ToString());

                JsonElement modifiedJsonElement = ConvertJObjectToJsonElement(modifiedJson);

                Content content = new()
                {
                    identity = modifiedJsonElement
                };

                demographicsQueryRequest.content = content;
            }
            else
            {
                demographicsQueryRequest.content = new Content() { identity = request.Content.Identity};
            }

            demographicsQueryRequest.SourceSystem = request.SourceSystem;
            demographicsQueryRequest.Agency = request.Agency;

            userRequestEntity = CreateUserRequest(demographicsQueryRequest, ApiCallType.VEIdentityExists, currentUser, trackingId, notificationOptions);

            var allowedSystemsQueryTask = _dataShareMappingRepository.GetAllowedDataShareMappingForSourceSystemAsync(request.SourceSystem);
            var demographicsQueryTask = DOH_DemographicQuery(userRequestEntity, demographicsQueryRequest, filterQueryResponse: false);

            // Wait for both tasks to complete
            await Task.WhenAll(allowedSystemsQueryTask, demographicsQueryTask);

            var allowedSystems = allowedSystemsQueryTask.Result;

            //Add source system to allowed system with full data sharing level.
            allowedSystems.Add(new DataShareMapping { AllowedSystemName = request.SourceSystem, DataSharingLevel = DataSharingLevel.Full.ToString() });

            if (demographicsQueryTask.Result is not DOH_DemographicQueryClientIdentityResponse demographicsQueryResult)
            {
                throw new Exception("Failed to retrieve demographics query result.");
            }

            if(!demographicsQueryResult.Success)
            {
                return new IdentityExistsResponse
                {
                    TrackingId = trackingId,
                    AuditId = demographicsQueryResult.AuditId,
                    Content = demographicsQueryResult.Content,
                    Success = demographicsQueryResult.Success,
                    Message = demographicsQueryResult.Message

                };
            }

            bool identityExistsInAllowedSystems = IdentityExists(demographicsQueryResult, allowedSystems);

            var identityExistsResponse = new IdentityExistsResponse
            {
                TrackingId = trackingId,
                AuditId = demographicsQueryResult.AuditId,
                Content = new IdentityExistsContent() { Exists = identityExistsInAllowedSystems },
                Success = true,
                Message = identityExistsInAllowedSystems ? "Identity found." : "No identity found."

            };

            return identityExistsResponse;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = request.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(IdentityExistsAsync),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(IdentityExistsAsync)}-Failed",
                TrackingId = request.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }
    }

    /// <summary>
    /// Check if identity exists in allowed systems
    /// </summary>
    /// <param name="demographicsQueryResponse"></param>
    /// <param name="allowedSystems"></param>
    /// <returns></returns>
    private static bool IdentityExists(DOH_DemographicQueryClientIdentityResponse demographicsQueryResponse, List<DataShareMapping> allowedSystems) 
    {
        JObject demographicsQueryContent = JObject.Parse(demographicsQueryResponse?.Content?.ToString());

        if( demographicsQueryContent == null || demographicsQueryContent.Count <= 0 ) 
        {
            return false;
        }
        if( demographicsQueryContent["identityGroupedBySource"] is not JArray SourceArray )
        {
            return false;
        }
        foreach( var source in SourceArray ) 
        {
            if( allowedSystems.Any( allowedSystem => allowedSystem.AllowedSystemName.Equals( source["source"]?["name"]?.ToString(), StringComparison.OrdinalIgnoreCase ) ) )
            {
                return true;
            }
        }

        return false;
    }

    public async Task<dynamic?> DOH_DemographicQuery( DOH_DemographicQueryRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions ) //,string responseIdentityFormatNames = "DEFAULT")
    {
        UserRequestEntity userRequestEntity = new();
        DOH_DemographicQueryRequest dOH_DemographicQueryRequest = new();
        var trackingId = string.Empty;

        try
        {            
            if (!(string.IsNullOrEmpty(filter.Trackingid)) && (filter.Trackingid.Length >= 1))
            {
                trackingId = filter.Trackingid.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.DOH_VEDemographicQuery.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";

            }
            if (filter.content.responseIdentityFormatNames == null || filter.content.responseIdentityFormatNames.Length == 0
                || filter.content.responseIdentityFormatNames.Any(view => string.IsNullOrWhiteSpace(view)))
            {
                filter.content.responseIdentityFormatNames = ["DEFAULT"];
            }

            string strIdentities = filter.content.identity.ToString();

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
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_DemographicQuery),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
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
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = filter.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_DemographicQuery),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
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

    public async Task<dynamic?> DOH_EnrichDemographicQuery(DOH_EnrichDemographicQueryRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        UserRequestEntity userRequestEntity = new();
        DOH_EnrichDemographicQueryRequest dOH_EnrichDemographicQueryRequest = new();
        var trackingId = string.Empty;

        try
        {
            trackingId = !string.IsNullOrEmpty(filter.TrackingId)
                ? filter.TrackingId.ToString()
                : $"{ApiCallType.DOH_VEEnrichDemographicQuery.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";

            if (filter.Content.ResponseIdentityFormatNames.IsNullOrEmpty() || filter.Content.ResponseIdentityFormatNames.Any(format => format.IsNullOrEmpty()))
            {
                filter.Content.ResponseIdentityFormatNames = ["DEFAULT"];
            }

            string strIdentities = filter.Content.Identity.ToString();

            if (strIdentities.Contains("null", StringComparison.CurrentCultureIgnoreCase))
            {
                dynamic modifiedJson = ReplaceNullValues(filter.Content.Identity.ToString());

                ContentEnrich content = new()
                {
                    Identity = ConvertJObjectToJsonElement(modifiedJson),
                    ResponseIdentityFormatNames = filter.Content.ResponseIdentityFormatNames
                };

                dOH_EnrichDemographicQueryRequest.Content = content;
            }
            else
            {
                dOH_EnrichDemographicQueryRequest.Content = filter.Content;
            }

            dOH_EnrichDemographicQueryRequest.SourceSystem = filter.SourceSystem;
            dOH_EnrichDemographicQueryRequest.Agency = filter.Agency;


            userRequestEntity = CreateUserRequest(dOH_EnrichDemographicQueryRequest, ApiCallType.DOH_VEEnrichDemographicQuery, currentUser, trackingId, notificationOptions);


            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEEnrichDemographicQuery, userRequestEntity);
                return trackingId;
            }

            return await DOH_EnrichDemographicQuery(userRequestEntity, dOH_EnrichDemographicQueryRequest);
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = filter.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_EnrichDemographicQuery),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_EnrichDemographicQuery)}-Failed",
                TrackingId = filter.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }
    }

    public async Task<dynamic?> DOH_LinkIdentities( DOH_LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new();
        try
        {
            if (!string.Equals(linkingSources.content.LinkToSource.Name, linkingSources.content.Source.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorResponseBuilder(linkingSources.TrackingId, "LinkToSource and Source do not match.");
            }

            if(!IsMatchingSourceSystem(linkingSources.content.LinkToSource.Name,linkingSources.SourceSystem))
            {
                return ErrorResponseBuilder(linkingSources.TrackingId, "Calling SourceSystem and LinkToSource do not match.");
            }

            if (!IsMatchingSourceSystem(linkingSources.content.Source.Name, linkingSources.SourceSystem))
            {
                return ErrorResponseBuilder(linkingSources.TrackingId, "Calling SourceSystem and Source do not match.");
            }

            var trackingId = string.Empty;
            if (!(string.IsNullOrEmpty(linkingSources.TrackingId)) && (linkingSources.TrackingId.Length >= 1))
            {
                trackingId = linkingSources.TrackingId.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.DOH_VELink.GetStringValue()}-{linkingSources.content.Source.GetTrackingId(linkingSources.content.LinkToSource)}";
            }
            userRequestEntity = CreateUserRequest(linkingSources, ApiCallType.DOH_VELink, currentUser, trackingId, notificationOptions);
            
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VELink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await DOH_LinkIdentities(userRequestEntity, linkingSources);
        }
        catch ( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = linkingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_LinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_LinkIdentities)}-Failed",
                TrackingId = linkingSources.TrackingId,
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
                Agency = linkingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_LinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_LinkIdentities)}-Failed",
                TrackingId = linkingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = linkingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_LinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_LinkIdentities)}-Failed",
                TrackingId = linkingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }
    }

    public async Task<dynamic?> DOH_UnLinkIdentities( DOH_UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new();
        try
        {
            if (!string.Equals(unLinkingSources.content.UnlinkFromSource.Name, unLinkingSources.content.Source.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorResponseBuilder(unLinkingSources.TrackingId, "UnlinkFromSource and Source do not match.");
            }

            if(!IsMatchingSourceSystem(unLinkingSources.content.UnlinkFromSource.Name, unLinkingSources.SourceSystem))
            {
                return ErrorResponseBuilder(unLinkingSources.TrackingId, "SourceSystem and UnlinkFromSource do not match.");
            }

            if(!IsMatchingSourceSystem(unLinkingSources.content.Source.Name, unLinkingSources.SourceSystem))
            {
                return ErrorResponseBuilder(unLinkingSources.TrackingId, "SourceSystem and Source do not match.");
            }
            var trackingId = string.Empty;
            if (!(string.IsNullOrEmpty(unLinkingSources.TrackingId)) && (unLinkingSources.TrackingId.Length >= 1))
            {
                trackingId = unLinkingSources.TrackingId.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.DOH_VEUnLink.GetStringValue()}-{unLinkingSources.content.Source.GetTrackingId(unLinkingSources.content.UnlinkFromSource)}";
            }
            userRequestEntity = CreateUserRequest(unLinkingSources, ApiCallType.DOH_VEUnLink, currentUser, trackingId, notificationOptions);
            
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEUnLink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unLinkingSources.UnlinkFromSource, unLinkingSources.Source);
            return await DOH_UnLinkIdentities(userRequestEntity, unLinkingSources);
        }
        catch ( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = unLinkingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_UnLinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_UnLinkIdentities)}-Failed",
                TrackingId = unLinkingSources.TrackingId,
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
                Agency = unLinkingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_UnLinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_UnLinkIdentities)}-Failed",
                TrackingId = unLinkingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = unLinkingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_UnLinkIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_UnLinkIdentities)}-Failed",
                TrackingId = unLinkingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }
    }

    public async Task<dynamic?> DOH_MergeIdentities( DOH_MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new();
        try
        {
            if (!string.Equals(mergingSources.content.ToSurviveSource.Name, mergingSources.content.ToRetireSource.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorResponseBuilder(mergingSources.TrackingId, "ToSurviveSource and ToRetireSource do not match.");
            }

            if(!IsMatchingSourceSystem(mergingSources.content.ToSurviveSource.Name, mergingSources.SourceSystem))
            {
                return ErrorResponseBuilder(mergingSources.TrackingId, "SourceSystem and ToSurviveSource do not match.");
            }

            if(!IsMatchingSourceSystem(mergingSources.content.ToRetireSource.Name, mergingSources.SourceSystem))
            {
                return ErrorResponseBuilder(mergingSources.TrackingId, "SourceSystem and ToRetireSource do not match.");
            }

            var trackingId = string.Empty;
            if (!(string.IsNullOrEmpty(mergingSources.TrackingId)) && (mergingSources.TrackingId.Length >= 1))
            {
                trackingId = mergingSources.TrackingId.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.DOH_VEMerge.GetStringValue()}-{mergingSources.content.ToSurviveSource.GetTrackingId(mergingSources.content.ToRetireSource)}";
            }
            userRequestEntity = CreateUserRequest(mergingSources, ApiCallType.DOH_VEMerge, currentUser, trackingId, notificationOptions);

            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);
            return await DOH_MergeIdentities(userRequestEntity, mergingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = mergingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_MergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_MergeIdentities)}-Failed",
                TrackingId = mergingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = mergingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_MergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_MergeIdentities)}-Failed",
                TrackingId = mergingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = mergingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_MergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_MergeIdentities)}-Failed",
                TrackingId = mergingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }
    }

    private static object ErrorResponseBuilder( string trackingId, string errorMessage ) 
        => new { errorCode = "400", message = errorMessage, success = false, trackingId = trackingId ?? "" };

    public async Task<dynamic?> DOH_UnMergeIdentities( DOH_UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new();
        try
        {
            if (!string.Equals(unMergingSources.content.UnmergeFromSource.Name, unMergingSources.content.UnmergeSource.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ErrorResponseBuilder(unMergingSources.TrackingId, "UnmergeFromSource and UnmergeSource do not match.");
            }

            if(!IsMatchingSourceSystem(unMergingSources.content.UnmergeFromSource.Name, unMergingSources.SourceSystem))
            {
                return ErrorResponseBuilder(unMergingSources.TrackingId, "SourceSystem and UnmergeFromSource do not match.");
            }

            if (!IsMatchingSourceSystem(unMergingSources.content.UnmergeSource.Name, unMergingSources.SourceSystem))
            {
                return ErrorResponseBuilder(unMergingSources.TrackingId, "SourceSystem and UnmergeSource do not match.");
            }
            var trackingId = string.Empty;
            if (!(string.IsNullOrEmpty(unMergingSources.TrackingId)) && (unMergingSources.TrackingId.Length >= 1))
            {
                trackingId = unMergingSources.TrackingId.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.DOH_VEUnMerge.GetStringValue()}-{unMergingSources.content.UnmergeSource.GetTrackingId(unMergingSources.content.UnmergeSource)}";
            }

            userRequestEntity = CreateUserRequest(unMergingSources, ApiCallType.DOH_VEUnMerge, currentUser, trackingId, notificationOptions);

            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEUnMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unMergingSources.UnmergeSource, unMergingSources.UnmergeFromSource);
            return await DOH_UnMergeIdentities(userRequestEntity, unMergingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = unMergingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_UnMergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_UnMergeIdentities)}-Failed",
                TrackingId = unMergingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = unMergingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_UnMergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_UnMergeIdentities)}-Failed",
                TrackingId = unMergingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = unMergingSources.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_UnMergeIdentities),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_UnMergeIdentities)}-Failed",
                TrackingId = unMergingSources.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

            throw;
        }
    }

    public async Task<dynamic?> DOH_DeleteSourceIdentity( DOH_DeleteClientIdentityRequest deleteSourceIdentity, string currentUser, ProcessType processType, NotificationOptions? notificationOptions )
    {
        UserRequestEntity userRequestEntity = new();
        try
        {
            if(!IsMatchingSourceSystem(deleteSourceIdentity.Content.Source.Name, deleteSourceIdentity.SourceSystem))
            {
                return ErrorResponseBuilder(deleteSourceIdentity.TrackingId, "SourceSystem and Source do not match.");
            }
            var trackingId = string.Empty;
            if (!(string.IsNullOrEmpty(deleteSourceIdentity.TrackingId)) && (deleteSourceIdentity.TrackingId.Length >= 1))
            {
                trackingId = deleteSourceIdentity.TrackingId.ToString();
            }
            else
            {
                trackingId = $"{ApiCallType.DOH_VEDelete.GetStringValue()}-{deleteSourceIdentity.Content.Source.GetTrackingId(deleteSourceIdentity.Content.Source)}";
            }
            userRequestEntity = CreateUserRequest(deleteSourceIdentity, ApiCallType.DOH_VEDelete, currentUser, trackingId, notificationOptions);

            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.DOH_VEDelete, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);

            return await DOH_DeleteSourceIdentity(userRequestEntity, deleteSourceIdentity);
        }
        catch ( HcaBadRequestException e )
        {
            UpdateProcessStatus( userRequestEntity, RequestStatus.Failed, e.Message );
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = deleteSourceIdentity.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_DeleteSourceIdentity),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DeleteSourceIdentity)}-Failed",
                TrackingId = deleteSourceIdentity.TrackingId,
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
                Agency = deleteSourceIdentity.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_DeleteSourceIdentity),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DeleteSourceIdentity)}-Failed",
                TrackingId = deleteSourceIdentity.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
        catch (Exception e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = currentUser,
                Agency = deleteSourceIdentity.Agency,
                Role = GetUserRoles(_httpContextAccessor.HttpContext),
                FunctionName = nameof(DOH_DeleteSourceIdentity),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_API}-{nameof(DOH_DeleteSourceIdentity)}-Failed",
                TrackingId = deleteSourceIdentity.TrackingId,
                Layer = ServiceLayer.API.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));

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

        var response = await _clientIdentityRequestExecutor.Execute<PostClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response.Content;
    }

    private async Task<dynamic?> DOH_PostIdentities( UserRequestEntity userRequestEntity, DOH_PostClientIdentityRequest request )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        // Always send the verato request with the GROUP_BY_SOURCE response identity format name
        DOH_PostIdentityRequestContent content = new(request.Content.Identity) {ResponseIdentityFormatNames = ["GROUP_BY_SOURCE"] };

        var linkIdentityRequest = new DOH_PostClientIdentityRequest(userRequestEntity.TrackingId)
        {
            protectedPopulation = request.protectedPopulation,
            SourceSystem = request.SourceSystem,
            Agency = request.Agency,
            Content = content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_PostClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        if (response.Success)
        {
            FilterPostResponse(request, response);
        }

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response;
    }

    private void FilterPostResponse( DOH_PostClientIdentityRequest request, DOH_PostClientIdentityResponse response )
    {
        string responseContent = Convert.ToString(response.Content);

        if (string.IsNullOrEmpty(responseContent))
        {
            response.Content = new JsonElement();
            return;
        }

        JObject jsonObjectResponse = JObject.Parse(responseContent);

        if( jsonObjectResponse.Count > 0 && jsonObjectResponse["identityGroupedBySource"] is JArray identityGroupedBySource )
        {
            JArray sources = [];
            foreach( var source in identityGroupedBySource )
            {
                if( IsMatchingSourceSystem( source["source"]?["name"]?.ToString(), request.SourceSystem ) )
                {
                    sources.Add( source );
                }
            }

            if( sources.Count > 0 )
            {
                jsonObjectResponse["identityGroupedBySource"] = sources;

                if( request.Content.ResponseIdentityFormatNames[0].ToString().Equals( "DEFAULT", StringComparison.CurrentCultureIgnoreCase ) )
                {
                    jsonObjectResponse["linkIdentity"] = TransformGroupedToDefault( jsonObjectResponse )["content"]?["identity"];
                    jsonObjectResponse.Remove( "identityGroupedBySource" );
                }
            }
            else
            {
                jsonObjectResponse = [];
            }
        }

        response.Content = ConvertJObjectToJsonElement( jsonObjectResponse ) ?? new();
    }

    private static bool IsMatchingSourceSystem(string? sourceSystemFromResponse, string? sourceSystemFromRequest)
    {
        if(string.Equals(sourceSystemFromRequest, sourceSystemFromResponse, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else if(string.Equals(sourceSystemFromRequest, sourceSystemFromResponse?.Split('.')[0], StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private async Task<LinkIdentitiesResponseContent?> LinkIdentities( UserRequestEntity userRequestEntity, LinkingSources linkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new LinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = linkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<LinkClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response.Content;
    }

    private async Task<List<PostIdentityResponseContent>?> DemographicSearch( UserRequestEntity userRequestEntity, Identity filter )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicSearchClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicSearchClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response.Content;
    }

    private async Task<dynamic?> DOH_DemographicSearch( UserRequestEntity userRequestEntity, DOH_DemographicsSearchRequest filter )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        // TODO: either create method for transforming requests, or put the request transform method in the Content object
        // Always send the verato request with the GROUP_BY_SOURCE response identity format name
        ContentSearch content = new() { identity = filter.content.identity, responseIdentityFormatNames = ["GROUP_BY_SOURCE"] };

        var demographicSearhRequest = new DOH_DemographicSearchClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = filter.SourceSystem,
            Agency = filter.Agency,
            Content = content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_DemographicSearchClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        if (response.Success)
        {
            FilterSearchResponse(filter, response);
        }

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

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
                if( FilterSearchResultGroupBySource( filter.SourceSystem, item ) is JToken filteredResult )
                {
                    filteredResults.Add( filteredResult );
                }
            }
        }
        else
        {
            foreach( var item in searchResults )
            {
                if ( FilterAndTransformSearchResultDefault( filter.SourceSystem, item ) is JToken filteredResult )
                {
                    filteredResults.Add( filteredResult );
                }
            }

            foreach( var result in filteredResults.OfType<JObject>() )
            {
                result.Remove( "identityGroupedBySource" );
            }
        }

        if (filteredResults == null || filteredResults.Count < 1)
        {
            response.Message = "No identity found.";
        }

        jsonObject["searchResults"] = filteredResults;
        response.Content = ConvertJObjectToJsonElement(jsonObject);
    }

    private static JArray GetSearchResultFilteredSources( string? sourceSystemName, JToken searchResult )
    {
        if( searchResult["identityGroupedBySource"] is not JArray identityGroupedBySourceArray ) { return []; }

        return (JArray)identityGroupedBySourceArray.Where(
            source => IsMatchingSourceSystem( source["source"]?["name"]?.ToString(), sourceSystemName )) ;
    }

    // TODO: check if shared logic can be extracted
    private static JToken? FilterAndTransformSearchResultDefault(string? sourceSystemName, JToken searchResult)
    {
        JArray sources = GetSearchResultFilteredSources(sourceSystemName, searchResult);
        
        if( sources.Count < 1 )
        {
            return null;
        }

        var identityJObject = new JObject
        {
            ["identityGroupedBySource"] = sources,
            ["linkId"] = searchResult["linkId"]
        };

        searchResult["identityGroupedBySource"]?.Replace( TransformGroupedToDefault( identityJObject, true ) );

        var grouped = searchResult["identityGroupedBySource"]?["identity"];
        if( grouped != null )
        {
            // Promote identity
            searchResult["identity"] = grouped.DeepClone();
        }

        return searchResult;
    }

    private static JToken? FilterSearchResultGroupBySource( string? sourceSystemName, JToken searchResult )
    {
        JArray sources = GetSearchResultFilteredSources(sourceSystemName, searchResult);

        if( sources.Count < 1 )
        {
            return null;
        }
        searchResult["identityGroupedBySource"] = sources;
        return searchResult;
    }

    private async Task<DemographicQueryResponseContent?> DemographicQuery( UserRequestEntity userRequestEntity, Identity filter )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicQueryClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response.Content;
    }

    // TODO: use inheritance to dedup filter/sourceSystem logic
    private async Task<dynamic?> DOH_DemographicQuery( UserRequestEntity userRequestEntity, DOH_DemographicQueryRequest filter, bool filterQueryResponse = true )
    {
        // Always send the verato request with the GROUP_BY_SOURCE response identity format name
        Content content = new Content() { identity = filter.content.identity, responseIdentityFormatNames = ["GROUP_BY_SOURCE"] };

        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DOH_DemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = filter.SourceSystem,
            Agency = filter.Agency,
            Content = content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_DemographicQueryClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        if (response.Success && filterQueryResponse)
        {
            FilterQueryResponse(filter, response);
        }

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response;
    }

    private async Task<dynamic?> DOH_EnrichDemographicQuery(UserRequestEntity userRequestEntity, DOH_EnrichDemographicQueryRequest filter)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var enrichDemographicSearhRequest = new DOH_EnrichDemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = filter.SourceSystem,
            Agency = filter.Agency,
            Content = filter.Content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_EnrichDemographicQueryClientIdentityResponse>(enrichDemographicSearhRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response;
    }

    private void FilterQueryResponse( DOH_DemographicQueryRequest filter, DOH_DemographicQueryClientIdentityResponse response )
    {
        JObject? jsonObject = JObject.Parse(response.Content.ToString());
        JArray? identityGroupedBySource = (JArray?)jsonObject["identityGroupedBySource"];

        if(jsonObject?.Count > 0 && identityGroupedBySource != null)
        {
            JArray sources = [];
            foreach( var source in identityGroupedBySource.Where(
                source => IsMatchingSourceSystem( source["source"]?["name"]?.ToString(), filter.SourceSystem ) ) ) 
            {
                sources.Add( source );
            }

            if (sources != null && sources.Count > 0)
            {
                jsonObject["identityGroupedBySource"] = sources;

                if (filter.content.responseIdentityFormatNames[0].ToString().Equals("DEFAULT", StringComparison.CurrentCultureIgnoreCase))
                {

                    jsonObject["identity"] = TransformGroupedToDefault(jsonObject)["content"]?["identity"];
                    jsonObject.Remove("identityGroupedBySource");

                }
            }
            else
            {
                jsonObject = null;
            }
        }

        if(jsonObject == null || jsonObject.Count < 1)
        {
            response.Message = "No identity found.";
        }

        // TODO: cover case where jsonObject is null
        response.Content = ConvertJObjectToJsonElement(jsonObject);
    }

    /// <summary>
    /// Transform the grouped response to the default format.
    /// </summary>
    /// <param name="groupedResponse"></param>
    /// <returns>identity response in default format</returns>
    private static JObject TransformGroupedToDefault(JObject groupedResponse, bool isTransformationForSearch = false)
    {
        var identityGroups = groupedResponse["identityGroupedBySource"] as JArray;

        if(identityGroups == null || identityGroups.Count == 0)
        {
            return new JObject
            {
                ["content"] = null
            };
        }
        else
        {
            var identity = new JObject
            {
                ["linkId"] = groupedResponse["linkId"],
                ["sources"] = new JArray()
            };

            //Core attributes
            var names = new JArray();
            var dobs = new JArray();
            var ssns = new JArray();
            var addresses = new JArray();
            var genders = new JArray();
            var emails = new JArray();
            var phones = new JArray();

            //custom attributes
            var customFields = new Dictionary<string, JArray>();

            foreach (var group in identityGroups)
            {
                // Source
                var source = group["source"];
                if( source != null )
                {
                    ((JArray)identity["sources"])?.Add( source );
                }

                // Names
                InsertGroupedItem( names, group, "names", "name" );

                //DOBs
                InsertGroupedItem(dobs, group, "datesOfBirth", "dateOfBirth");

                //SSNs
                InsertGroupedItem(ssns, group, "ssns", "ssn");

                //Addresses
                InsertGroupedItem(addresses, group, "addresses", "address");

                //Genders
                InsertGroupedItem(genders, group, "genders", "gender");

                //Emails
                InsertGroupedItem(emails, group, "emails", "email");

                //Phone Numbers
                InsertGroupedItem(phones, group, "phoneNumbers", "phoneNumber");

                // Dynamic custom.* handling
                foreach ( var property in group.Children<JProperty>() )
                {
                    if( property.Name.StartsWith( "custom." ) )
                    {
                        var customArray = property.Value as JArray;
                        if( customArray != null )
                        {
                            foreach( var item in customArray )
                            {
                                var innerValue = item[property.Name];
                                if( innerValue != null )
                                {
                                    if( !customFields.ContainsKey( property.Name ) )
                                        customFields[property.Name] = new JArray();

                                    customFields[property.Name].Add( innerValue );
                                }
                            }
                        }
                    }
                }
            }

            // Add all to identity
            identity["names"] = names;
            identity["datesOfBirth"] = dobs;
            identity["ssns"] = ssns;
            identity["addresses"] = addresses;
            identity["genders"] = genders;
            identity["emails"] = emails;
            identity["phoneNumbers"] = phones;

            // Assign custom fields
            foreach (var custom in customFields)
            {
                identity[custom.Key] = custom.Value;
            }

            if (isTransformationForSearch)
            {
                return new JObject
                {

                    ["linkId"] = groupedResponse["linkId"],
                    ["identity"] = identity

                };
            }
            else
            {
                return new JObject
                {
                    ["content"] = new JObject
                    {
                        ["linkId"] = groupedResponse["linkId"],
                        ["identity"] = identity
                    }
                };
            }

        }
    }

    private static void InsertGroupedItem( JArray names, JToken group, string groupName, string itemName )
    {
        foreach( var item in group[groupName] ?? new JArray() )
        {
            var name = item[itemName];
            if( name != null )
            {
                names.Add( name );
            }
        }
    }

    private async Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities( UserRequestEntity userRequestEntity, UnLinkingSources unLinkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var unLinkClientIdentityRequest = new UnLinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unLinkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<UnLinkClientIdentityResponse>(unLinkClientIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response.Content;
    }

    private async Task<MergeIdentitiesResponseContent?> MergeIdentities( UserRequestEntity userRequestEntity, MergingSources mergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var mergeClientIdentityRequest = new MergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = mergingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<MergeClientIdentityResponse>(mergeClientIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response.Content;
    }

    private async Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities( UserRequestEntity userRequestEntity, UnMergingSources unMergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var unMergeClientIdentityRequest = new UnMergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unMergingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<UnMergeClientIdentityResponse>(unMergeClientIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response.Content;
    }

    private async Task<dynamic?> DOH_LinkIdentities( UserRequestEntity userRequestEntity, DOH_LinkingSources linkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new DOH_LinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = linkingSources.SourceSystem,
            Agency = linkingSources.Agency,
            Content = linkingSources.content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_LinkClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response;
    }

    private async Task<dynamic?> DOH_UnLinkIdentities( UserRequestEntity userRequestEntity, DOH_UnLinkingSources unLinkingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var unLinkClientIdentityRequest = new DOH_UnLinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = unLinkingSources.SourceSystem,
            Agency = unLinkingSources.Agency,
            Content = unLinkingSources.content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_UnLinkClientIdentityResponse>(unLinkClientIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response;
    }

    private async Task<dynamic?> DOH_MergeIdentities( UserRequestEntity userRequestEntity, DOH_MergingSources mergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var mergeClientIdentityRequest = new DOH_MergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = mergingSources.SourceSystem,
            Agency = mergingSources.Agency,
            Content = mergingSources.content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_MergeClientIdentityResponse>(mergeClientIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response;
    }

    private async Task<dynamic?> DOH_DeleteSourceIdentity( UserRequestEntity userRequestEntity, DOH_DeleteClientIdentityRequest deleteSourceIdentity )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var deleteClientIdentityRequest = new DOH_DeleteClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = deleteSourceIdentity.SourceSystem,
            Agency = deleteSourceIdentity.Agency,
            Content = deleteSourceIdentity.Content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_DeleteClientIdentityResponse>(deleteClientIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

        return response;
    }

    private async Task<dynamic?> DOH_UnMergeIdentities( UserRequestEntity userRequestEntity, DOH_UnMergingSources unMergingSources )
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var unMergeClientIdentityRequest = new DOH_UnMergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            SourceSystem = unMergingSources.SourceSystem,
            Agency = unMergingSources.Agency,
            Content = unMergingSources.content,
            Caller = ServiceLayer.API.ToString()
        };

        var response = await _clientIdentityRequestExecutor.Execute<DOH_UnMergeClientIdentityResponse>(unMergeClientIdentityRequest, requestStatusUpdater)
            ?? throw new HcaBadRequestException("Failed to process request");

        // Determine request status and message based on the response
        (RequestStatus requestStatus, string requestMessage) = response.Success
            ? (RequestStatus.Success, "Request Processed Successfully")
            : (RequestStatus.Failed, response.Message);

        // Update the response in the user request entity
        userRequestEntity.ResponseJson = JsonSerializer.Serialize(response);

        // Update the process status
        UpdateProcessStatus(userRequestEntity, requestStatus, requestMessage);

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

    /// <summary>
    /// Get Role claim for user in context
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static string? GetUserRoles(HttpContext? context)
    {
        if (context?.User != null && context.User.Claims.Any(c => c.Type == ClaimTypes.Role))
        {
            return context?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }

        return string.Empty;
    }

    /// <summary>
    /// Get Source System Access from context
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static async Task<(bool, List<string>)> GetSourceSysteAccessFromContext(HttpContext? context)
    {
        var groups = context?.User.FindAll(ClaimTypes.GroupSid).Select(c => c.Value).ToList();
        if (groups != null)
        {
            var roles = await _appRoleMappingRepository.GetAppRoleMappingsAsync(groups);
            if (roles != null)
            {
                if (roles.Any(role => (role.AppRole.AppRoleName == AppRole.MPIAdmin.ToString())
                || (role.AppRole.AppRoleName == AppRole.MPIReader.ToString())))
                {
                    return (true, new List<string>());
                }
                else
                {
                    return (false, roles.Select(role => role.System.SourceSystemName).ToList());
                }
            }
        }

        return (false, new List<string>());
    }

}