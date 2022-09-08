using HCA.Core.Processors;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Http;
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

        public MuleSoftRequestExecuter(MuleSoftRetryOptions muleSoftRetryOptions, IDelayCaculator delayCaculator,
            IMuleSoftRepository muleSoftRepository, IMuleSoftRequestBuilder muleSoftRequestBuilder)
        {
            _muleSoftRetryOptions = muleSoftRetryOptions;
            _delayCaculator = delayCaculator;
            _muleSoftRepository = muleSoftRepository;
            _muleSoftRequestBuilder = muleSoftRequestBuilder;
            requestExecuters = BuildRequestExecutors();
        }

        public async Task<T?> Execute<T>(BaseRequest request, IRequestStatusUpdater statusUpdater) where T : BaseResponse
        {
            
            for (int i = 0; i < _muleSoftRetryOptions.MaxRetries; ++i)
            {
                try
                {
                    if (i > 0)
                        await statusUpdater.UpdateStatus(request, RequestStatus.Retrying, $"Retrying request, iteration {i}");

                    var response = await requestExecuters[request.ApiCallType](request);
                    if (response.Success) return response as T;

                    if (HasErrors(response))
                    {
                        var errorMessage = response.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
                        await statusUpdater.UpdateStatus(request, RequestStatus.Failed, $"{errorMessage}");
                        throw new HcaMuleSoftException(errorMessage);
                    }

                    await Task.Delay(_delayCaculator.Calculate(i + 1));
                }
                catch (HcaHttpException e)
                {
                    if (!_muleSoftRetryOptions.ReTriableStatusCodes.Contains(e.StatusCode)) throw;
                    await Task.Delay(_delayCaculator.Calculate(i + 1));
                }
            }

            throw new HcaMuleSoftException("Error processing the request");
        }

        private IDictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>> BuildRequestExecutors()
        {
            var requestExecuters = new Dictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>>
            {
                [ ApiCallType.VEPost ] = PostIdentity,
                [ ApiCallType.VELink ] = LinkIdentities,
                [ ApiCallType.VEUnLink ] = UnLinkIdentities,
                [ ApiCallType.VEMerge ] = MergeIdentities,
                [ ApiCallType.VEUnMerge ] = UnMergeIdentities,
                [ ApiCallType.VEDemographicSearch ] = DemographicSearch
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

        private async Task<BaseResponse> PostIdentity(BaseRequest request)
        {
            var postidentityRequest = Cast<PostClientIdentityRequest>(request);
            var muleSoftRequest = _muleSoftRequestBuilder.BuildPostIdentityRequest(postidentityRequest);
            var muleSoftResponse = await _muleSoftRepository.PostIdentity(muleSoftRequest);
            var response = CreateResponse<PostClientIdentityResponse>(muleSoftResponse);
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

