using HCA.Core.Processors;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Http;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.MuleSoft.Request;
using HCA.Models.MuleSoft.Response;
using HCA.Models.Request;
using HCA.Models.Response;
using HCA.MuleSoft;

namespace HCA.Core.Services
{
    public class MuleSoftRequestExecuter : IMuleSoftRequestExecuter
    {
        private readonly MuleSoftRetryOptions _muleSoftRetryOptions;
        private readonly IDelayCaculator _delayCaculator;
        private readonly IMuleSoftRepository _muleSoftRepository;
        private readonly IMuleSoftRequestBuilder _muleSoftRequestBuilder;
        private readonly IDictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>> requestExecuters;
        private readonly IAppLogger _appLogger;

        public MuleSoftRequestExecuter(MuleSoftRetryOptions muleSoftRetryOptions, IDelayCaculator delayCaculator,
            IMuleSoftRepository muleSoftRepository, IMuleSoftRequestBuilder muleSoftRequestBuilder, IAppLogger appLogger)
        {
            _muleSoftRetryOptions = muleSoftRetryOptions;
            _delayCaculator = delayCaculator;
            _muleSoftRepository = muleSoftRepository;
            _muleSoftRequestBuilder = muleSoftRequestBuilder;
            requestExecuters = BuildRequestExecutors();
            _appLogger = appLogger;
        }

        public async Task<T?> Execute<T>(BaseRequest request, IRequestStatusUpdater statusUpdater) where T : BaseResponse
        {
            var exception = "Error processing the request";
            for (int i = 0; i < _muleSoftRetryOptions.MaxRetries; ++i)
            {
                try
                {
                    //if (i > 0)
                    //    await statusUpdater.UpdateStatus(request, RequestStatus.Retrying, $"Retrying request, iteration {i}");

                    if(i > 0)
                        _appLogger.LogInformation($"Retrying request {request.TrackingId}, iteration{i}");

                    var response = await requestExecuters[request.ApiCallType](request);
                    return response as T;
                    //if (response.Success) return response as T;

                    //if (HasErrors(response))
                    //{
                    //    var errorMessage = response.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
                    //    await statusUpdater.UpdateStatus(request, RequestStatus.Failed, $"{errorMessage}");
                    //    throw new HcaMuleSoftException(errorMessage);
                    //}

                    //await Task.Delay(_delayCaculator.Calculate(i + 1));
                }
                //catch(HcaMuleSoftException e)
                //{
                //    throw;
                //}
                catch (HcaHttpException e)
                {
                    _appLogger.LogInformation($"Retrying for the exception HcaHttpException {e.StatusCode}");
                    _appLogger.LogError(e);
                    exception = e.ToString();
                    if (!_muleSoftRetryOptions.ReTriableStatusCode.Contains(e.StatusCode)) throw new HcaMuleSoftException(exception);
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

            throw new HcaMuleSoftException(exception);
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
                [ApiCallType.DOH_VEDelete] = DOH_DeleteIdentity
            };

            return requestExecuters;
        }

        private async Task<BaseResponse> LinkIdentities(BaseRequest request)
        {
            var linkIdentitiesRequest = Cast<LinkClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildLinkIdentitisRequest(linkIdentitiesRequest);
            var muleSoftResponse = await _muleSoftRepository.LinkIdentities(muleSoftRequest);
            var response = CreateResponse<LinkClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> UnLinkIdentities(BaseRequest request)
        {
            var unLinkClientIdentityRequest = Cast<UnLinkClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildUnLinkIdentitiesRequest(unLinkClientIdentityRequest);
            var muleSoftResponse = await _muleSoftRepository.UnLinkIdentities(muleSoftRequest);
            var response = CreateResponse<UnLinkClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DeleteIdentity(BaseRequest request)
        {
            var deleteIdentityRequest = Cast<DeleteClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildDeleteIdentityRequest(deleteIdentityRequest);
            var muleSoftResponse = await _muleSoftRepository.DeleteIdentity(muleSoftRequest);
            var response = CreateResponse<DeleteClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_DeleteIdentity(BaseRequest request)
        {
            var deleteIdentityRequest = Cast<DOH_DeleteSourceIdentityRequest>(request);
            DeleteIdentyRequest muleSoftRequest = new DeleteIdentyRequest(deleteIdentityRequest.TrackingId, deleteIdentityRequest.Content);
            var muleSoftResponse = await _muleSoftRepository.DOH_DeleteSourceIdentities(muleSoftRequest);
            var response = CreateResponse<DOH_DeleteSourceIdentityResponse>(muleSoftResponse.Content);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> MergeIdentities(BaseRequest request)
        {
            var mergeClientIdentityRequest = Cast<MergeClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildMergeIdentitiesRequest(mergeClientIdentityRequest);
            var muleSoftResponse = await _muleSoftRepository.MergeIdentities(muleSoftRequest);
            var response = CreateResponse<MergeClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> UnMergeIdentities(BaseRequest request)
        {
            var unmergeClientIdentityRequest = Cast<UnMergeClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildUnMergeIdentitiesRequest(unmergeClientIdentityRequest);
            var muleSoftResponse = await _muleSoftRepository.UnMergeIdentities(muleSoftRequest);
            var response = CreateResponse<UnMergeClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_LinkIdentities(BaseRequest request)
        {
            var linkIdentitiesRequest = Cast<DOH_LinkClientIdentityRequest>(request);
            LinkIdentitiesRequest muleSoftRequest = new LinkIdentitiesRequest(linkIdentitiesRequest.TrackingId,linkIdentitiesRequest.Content);
            var muleSoftResponse = await _muleSoftRepository.DOH_LinkIdentities(muleSoftRequest);
            var response = CreateResponse<DOH_LinkClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_UnLinkIdentities(BaseRequest request)
        {
            var unLinkClientIdentityRequest = Cast<DOH_UnLinkClientIdentityRequest>(request);
            UnLinkIdentitiesRequest muleSoftRequest = new(unLinkClientIdentityRequest.TrackingId, unLinkClientIdentityRequest.Content);
            var muleSoftResponse = await _muleSoftRepository.DOH_UnLinkIdentities(muleSoftRequest);
            var response = CreateResponse<DOH_UnLinkClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_MergeIdentities(BaseRequest request)
        {
            var mergeClientIdentityRequest = Cast<DOH_MergeClientIdentityRequest>(request);
            MergeIdentitiesRequest muleSoftRequest = new(mergeClientIdentityRequest.TrackingId, mergeClientIdentityRequest.Content);
            var muleSoftResponse = await _muleSoftRepository.DOH_MergeIdentities(muleSoftRequest);
            var response = CreateResponse<DOH_MergeClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_UnMergeIdentities(BaseRequest request)
        {
            var unmergeClientIdentityRequest = Cast<DOH_UnMergeClientIdentityRequest>(request);
            UnMergeIdentitiesRequest muleSoftRequest = new(unmergeClientIdentityRequest.TrackingId, unmergeClientIdentityRequest.Content);

            var muleSoftResponse = await _muleSoftRepository.DOH_UnMergeIdentities(muleSoftRequest);
            var response = CreateResponse<DOH_UnMergeClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> PostIdentity(BaseRequest request)
        {
            var postidentityRequest = Cast<PostClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildPostIdentityRequest(postidentityRequest);
            var muleSoftResponse = await _muleSoftRepository.PostIdentity(muleSoftRequest);
            var response = CreateResponse<PostClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_PostIdentity(BaseRequest request)
        {
            var postidentityRequest = Cast<DOH_PostClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildDOH_PostIdentityRequest(postidentityRequest);

            var muleSoftResponse = await _muleSoftRepository.DOH_PostIdentity(muleSoftRequest);
            var response = CreateResponse<DOH_PostClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DemographicSearch(BaseRequest request)
        {
            var searchRequest = Cast<DemographicSearchClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildDemographicSearchRequest(searchRequest);
            var muleSoftResponse = await _muleSoftRepository.DemographicSearch(muleSoftRequest);
            var response = CreateResponse<DemographicSearchClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content.SearchResults;
            return response;
        }

        private async Task<BaseResponse> DOH_DemographicSearch(BaseRequest request)
        {
            var searchRequest = Cast<DOH_DemographicSearchClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildDOH_DemographicSearchRequest(searchRequest);
            var muleSoftResponse = await _muleSoftRepository.DOH_DemographicSearch(muleSoftRequest);
            var response = CreateResponse<DOH_DemographicSearchClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DemographicQuery(BaseRequest request)
        {
            var searchRequest = Cast<DemographicQueryClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildDemographicQueryRequest(searchRequest);
            var muleSoftResponse = await _muleSoftRepository.DemographicQuery(muleSoftRequest);
            var response = CreateResponse<DemographicQueryClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private async Task<BaseResponse> DOH_DemographicQuery(BaseRequest request)
        {
            var searchRequest = Cast<DOH_DemographicQueryClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildDOH_DemographicQueryRequest(searchRequest);
            var muleSoftResponse = await _muleSoftRepository.DOH_DemographicQuery(muleSoftRequest);
            var response = CreateResponse<DOH_DemographicQueryClientIdentityResponse>(muleSoftResponse);
            response.Content = muleSoftResponse.Content;
            return response;
        }

        private static T CreateResponse<T>(MuleSoftResponse muleSoftResponse) where T : BaseResponse, new()
        {
            var value = new T
            {
                TrackingId = muleSoftResponse.TrackingId,
                AuditId = muleSoftResponse.AuditId,
                Success = muleSoftResponse.Success,
                RetryableError = muleSoftResponse.RetryableError,
                Message = muleSoftResponse.Message,
                Errors = muleSoftResponse.Errors
            };
            return value;
        }

        private static T Cast<T>(BaseRequest request) where T : BaseRequest
        {
            if (request is not T muleSoftRequest)
                throw new HcaMuleSoftException("Invalid input");
            return muleSoftRequest;
        }

        private static bool HasErrors(BaseResponse response)
        {
            if (null != response.Errors && response.Errors.Count > 0) return true;
            if (null == response) return true;
            return false;
        }
    }
}

