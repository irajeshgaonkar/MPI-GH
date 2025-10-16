using HCA.Core.Processors;
using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Http;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.Request;
using HCA.Models.Response;
using HCA.Models.Verato.Request;
using HCA.Models.Verato.Response;
using HCA.Verato;
using HCA.Verato.Options;

namespace HCA.Core.Services
{
    public class VeratoRequestExecuter : IVeratoRequestExecuter
    {
        private readonly AppSettings _appSettings;
        private readonly IDelayCaculator _delayCaculator;
        private readonly IVeratoRepository _veratoRepository;
        private readonly IVeratoRequestBuilder _veratoRequestBuilder;
        private readonly IDictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>> requestExecuters;
        private readonly IAppLogger _appLogger;

        public VeratoRequestExecuter(AppSettings appSettings, IDelayCaculator delayCaculator,
            IVeratoRepository veratoRepository, IVeratoRequestBuilder veratoRequestBuilder, IAppLogger appLogger)
        {
            _appSettings = appSettings;
            _delayCaculator = delayCaculator;
            _veratoRepository = veratoRepository;
            _veratoRequestBuilder = veratoRequestBuilder;
            requestExecuters = BuildRequestExecutors();
            _appLogger = appLogger;
        }

        public async Task<T?> Execute<T>(BaseRequest request, IRequestStatusUpdater statusUpdater) where T : BaseResponse
        {
            var exception = "Error processing the request";
            for (int i = 0; i < _appSettings.VeratoOptions.RetryOptions.MaxRetries; ++i)
            {
                try
                {
                    //if (i > 0)
                    //    await statusUpdater.UpdateStatus(request, RequestStatus.Retrying, $"Retrying request, iteration {i}");

                    if( i > 0 )
                    {
                        _appLogger.LogInformation($"Retrying request {request.TrackingId}, iteration{i}");
                    }

                    var response = await requestExecuters[request.ApiCallType](request);
                    return response as T;
                    //if (response.Success) return response as T;

                    //if (HasErrors(response))
                    //{
                    //    var errorMessage = response.Errors?.JoinBy("|") ?? "Error occured while posting request to Verato";
                    //    await statusUpdater.UpdateStatus(request, RequestStatus.Failed, $"{errorMessage}");
                    //    throw new HcaVeratoException(errorMessage);
                    //}

                    //await Task.Delay(_delayCaculator.Calculate(i + 1));
                }
                //catch(HcaVeratoException e)
                //{
                //    throw;
                //}
                catch (HcaHttpException e)
                {
                    _appLogger.LogInformation($"Retrying for the exception HcaHttpException {e.StatusCode}");
                    _appLogger.LogError(e);
                    exception = e.ToString();

                    if (!_appSettings.VeratoOptions.RetryOptions.ReTriableStatusCode.Contains(e.StatusCode)) throw new HcaVeratoException(exception);

                    await Task.Delay(_delayCaculator.Calculate(i + 1));
                }
                catch(Exception e)
                {
                    _appLogger.LogInformation($"Retrying for the exception Exception");
                    _appLogger.LogError(e);
                    exception = e.Message;
                    await Task.Delay(_delayCaculator.Calculate(i + 1));
                }
            }

            throw new HcaVeratoException(exception);
        }

        private IDictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>> BuildRequestExecutors()
        {
            var requestExecuters = new Dictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>>
            {
                [ ApiCallType.VEPost ] = PostIdentity,
                [ApiCallType.DOH_VEPost] = DOH_PostIdentity,
                [ ApiCallType.VELink ] = LinkIdentities,
                [ ApiCallType.DOH_VELink] = DOH_LinkIdentities,
                [ ApiCallType.VEUnLink ] = UnLinkIdentities,
                [ ApiCallType.DOH_VEUnLink] = DOH_UnLinkIdentities,
                [ ApiCallType.VEMerge ] = MergeIdentities,
                [ ApiCallType.DOH_VEMerge] = DOH_MergeIdentities,
                [ ApiCallType.VEUnMerge ] = UnMergeIdentities,
                [ ApiCallType.DOH_VEUnMerge] = DOH_UnMergeIdentities,
                [ ApiCallType.VEDemographicSearch ] = DemographicSearch,
                [ ApiCallType.DOH_VEDemographicSearch ] = DOH_DemographicSearch,
                [ApiCallType.VEDemographicQuery] = DemographicQuery,
                [ApiCallType.DOH_VEDemographicQuery] = DOH_DemographicQuery,
                [ApiCallType.VEDelete] = DeleteIdentity,
                [ApiCallType.DOH_VEDelete] = DOH_DeleteIdentity,
                [ApiCallType.DOH_VEEnrichDemographicQuery] = DOH_EnrichDemographicQuery,
                [ApiCallType.VENativeIdQuery] = NativeIdQuery,
                [ApiCallType.VESearchNotifications] = SearchNotifications,
            };

            return requestExecuters;
        }

        private async Task<BaseResponse> LinkIdentities(BaseRequest request)
        {
            var linkIdentitiesRequest = Cast<LinkClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildLinkIdentitisRequest(linkIdentitiesRequest);
            var veratoResponse = await _veratoRepository.LinkIdentities(veratoRequest);
            var response = CreateResponse<LinkClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> UnLinkIdentities(BaseRequest request)
        {
            var unLinkClientIdentityRequest = Cast<UnLinkClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildUnLinkIdentitiesRequest(unLinkClientIdentityRequest);
            var veratoResponse = await _veratoRepository.UnLinkIdentities(veratoRequest);
            var response = CreateResponse<UnLinkClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DeleteIdentity(BaseRequest request)
        {
            var deleteIdentityRequest = Cast<DeleteClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildDeleteIdentityRequest(deleteIdentityRequest);
            var veratoResponse = await _veratoRepository.DeleteIdentity(veratoRequest);
            var response = CreateResponse<DeleteClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_DeleteIdentity(BaseRequest request)
        {
            var deleteIdentityRequest = Cast<DOH_DeleteClientIdentityRequest>(request);
            DeleteIdentyRequestContent content = new DeleteIdentyRequestContent(deleteIdentityRequest.Content.Source);
            DeleteIdentyRequest veratoRequest = new DeleteIdentyRequest(deleteIdentityRequest.TrackingId, content);
            var veratoResponse = await _veratoRepository.DOH_DeleteSourceIdentities(veratoRequest);
            var response = CreateResponse<DOH_DeleteClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> MergeIdentities(BaseRequest request)
        {
            var mergeClientIdentityRequest = Cast<MergeClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildMergeIdentitiesRequest(mergeClientIdentityRequest);
            var veratoResponse = await _veratoRepository.MergeIdentities(veratoRequest);
            var response = CreateResponse<MergeClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> UnMergeIdentities(BaseRequest request)
        {
            var unmergeClientIdentityRequest = Cast<UnMergeClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildUnMergeIdentitiesRequest(unmergeClientIdentityRequest);
            var veratoResponse = await _veratoRepository.UnMergeIdentities(veratoRequest);
            var response = CreateResponse<UnMergeClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_LinkIdentities(BaseRequest request)
        {
            var linkIdentitiesRequest = Cast<DOH_LinkClientIdentityRequest>(request);
            LinkIdentitiesRequest veratoRequest = new LinkIdentitiesRequest(linkIdentitiesRequest.TrackingId,linkIdentitiesRequest.Content);
            var veratoResponse = await _veratoRepository.DOH_LinkIdentities(veratoRequest);
            var response = CreateResponse<DOH_LinkClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_UnLinkIdentities(BaseRequest request)
        {
            var unLinkClientIdentityRequest = Cast<DOH_UnLinkClientIdentityRequest>(request);
            UnLinkIdentitiesRequest veratoRequest = new(unLinkClientIdentityRequest.TrackingId, unLinkClientIdentityRequest.Content);
            var veratoResponse = await _veratoRepository.DOH_UnLinkIdentities(veratoRequest);
            var response = CreateResponse<DOH_UnLinkClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_MergeIdentities(BaseRequest request)
        {
            var mergeClientIdentityRequest = Cast<DOH_MergeClientIdentityRequest>(request);
            MergeIdentitiesRequest veratoRequest = new(mergeClientIdentityRequest.TrackingId, mergeClientIdentityRequest.Content);
            var veratoResponse = await _veratoRepository.DOH_MergeIdentities(veratoRequest);
            var response = CreateResponse<DOH_MergeClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_UnMergeIdentities(BaseRequest request)
        {
            var unmergeClientIdentityRequest = Cast<DOH_UnMergeClientIdentityRequest>(request);
            UnMergeIdentitiesRequest veratoRequest = new(unmergeClientIdentityRequest.TrackingId, unmergeClientIdentityRequest.Content);

            var veratoResponse = await _veratoRepository.DOH_UnMergeIdentities(veratoRequest);
            var response = CreateResponse<DOH_UnMergeClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> PostIdentity(BaseRequest request)
        {
            var postidentityRequest = Cast<PostClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildPostIdentityRequest(postidentityRequest);
            var veratoResponse = await _veratoRepository.PostIdentity(veratoRequest);
            var response = CreateResponse<PostClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_PostIdentity(BaseRequest request)
        {
            var postidentityRequest = Cast<DOH_PostClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildDOH_PostIdentityRequest(postidentityRequest);

            var veratoResponse = await _veratoRepository.DOH_PostIdentity(veratoRequest);
            var response = CreateResponse<DOH_PostClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DemographicSearch(BaseRequest request)
        {
            var searchRequest = Cast<DemographicSearchClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildDemographicSearchRequest(searchRequest);
            var veratoResponse = await _veratoRepository.DemographicSearch(veratoRequest);
            var response = CreateResponse<DemographicSearchClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content.SearchResults;
            return response;
        }

        private async Task<BaseResponse> DOH_DemographicSearch(BaseRequest request)
        {
            var searchRequest = Cast<DOH_DemographicSearchClientIdentityRequest>(request);
            var veratoRequest = new DemographicSearchRequest(searchRequest.TrackingId, searchRequest.Content);  //_veratoRequestBuilder.BuildDOH_DemographicSearchRequest(searchRequest);
            var veratoResponse = await _veratoRepository.CallVerato< DOH_DemographicSearchClientIdentityResponse>(VeratoEndpoint.DemographicSearch,veratoRequest);
            //var response = CreateResponse<DOH_DemographicSearchClientIdentityResponse>(veratoResponse);
            //response.Content = veratoResponse.Content;
            return veratoResponse;
        }

        private async Task<BaseResponse> DemographicQuery(BaseRequest request)
        {
            var searchRequest = Cast<DemographicQueryClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildDemographicQueryRequest(searchRequest);
            var veratoResponse = await _veratoRepository.DemographicQuery(veratoRequest);
            var response = CreateResponse<DemographicQueryClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_DemographicQuery(BaseRequest request)
        {
            var searchRequest = Cast<DOH_DemographicQueryClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildDOH_DemographicQueryRequest(searchRequest);
            var veratoResponse = await _veratoRepository.DOH_DemographicQuery(veratoRequest);
            var response = CreateResponse<DOH_DemographicQueryClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_EnrichDemographicQuery(BaseRequest request)
        {
            var queryEnrichRequest = Cast<DOH_EnrichDemographicQueryClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.BuildDOH_EnrichDemographicQueryRequest(queryEnrichRequest);
            var veratoResponse = await _veratoRepository.DOH_EnrichDemographicQuery(veratoRequest);
            var response = CreateResponse<DOH_EnrichDemographicQueryClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> NativeIdQuery(BaseRequest request)
        {
            var queryRequest = Cast<NativeIdQueryClientIdentityRequest>(request);
            var veratoRequest = _veratoRequestBuilder.Build_NativeIdQueryRequest(queryRequest);
            var veratoResponse = await _veratoRepository.NativeIdQuery(veratoRequest);
            var response = CreateResponse<NativeIdQueryClientIdentityResponse>(veratoResponse);
            response.Content = veratoResponse.Content;
            return response;
        }

        private async Task<BaseResponse> SearchNotifications(BaseRequest request)
        {
            var queryRequest = Cast<SearchClientIdentityNotificationsRequest>(request);
            var veratoRequest = _veratoRequestBuilder.Build_SearchNotificationsRequest(queryRequest);
            var veratoResponse = await _veratoRepository.SearchNotifications(veratoRequest);
            var response = CreateResponse<SearchClientIdentityNotificationResponse>(veratoResponse);
            response.Content.TotalElements = veratoResponse.Content.TotalElements;
            response.Content.HasNext = veratoResponse.Content.HasNext;
            response.Content.CustomerId = veratoResponse.Content.CustomerId;
            response.Content.Notifications = new();
            if (veratoResponse.Content.Notifications != null)
            {
                foreach (var notification in veratoResponse.Content.Notifications)
                {
                    response.Content.Notifications.Add(new ClientIdentityNotification
                    {
                        Ts = notification.Ts,
                        Service = notification.Service,
                        NotificationType = notification.NotificationType,
                        Body = notification.Body,
                        Username = notification.Username
                    });
                }
            }
            return response;
        }

        private static T CreateResponse<T>(VeratoResponse veratoResponse) where T : BaseResponse, new()
        {
            var value = new T
            {
                TrackingId = veratoResponse.TrackingId,
                AuditId = veratoResponse.AuditId,
                Success = veratoResponse.Success,
                RetryableError = veratoResponse.RetryableError,
                Message = veratoResponse.Message,
                Errors = veratoResponse.Errors
            };
            return value;
        }

        private static T Cast<T>(BaseRequest request) where T : BaseRequest
        {
            if( request is not T veratoRequest )
            {
                throw new HcaVeratoException("Invalid input");
            }

            return veratoRequest;
        }

        private static bool HasErrors(BaseResponse response)
        {
            if (null != response.Errors && response.Errors.Count > 0)
            {
                return true;
            }

            return null == response;
        }
    }
}

