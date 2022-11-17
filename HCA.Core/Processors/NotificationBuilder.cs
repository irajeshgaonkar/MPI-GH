using HCA.Infrastructure.Extensions;
using HCA.Models.DynamoDb;
using HCA.Models.Request;
using HCA.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Core.Processors
{
    public class NotificationBuilder
    {

        public HcaMpiNotification BuildPostIdentityNotification(PostClientIdentityRequest request, PostClientIdentityResponse? response)
        {
            return new HcaMpiNotification()
            {
                LinkId = response?.Content.LinkId ?? "Link Id not Generated",
                TrackingId = request.TrackingId,
                TimeStamp = DateTime.Now,
                SourceSystemName = request.Content.First().SourceSystemName,
                SourceSystemId = request.Content.First().SourceSystemId,
                Operation = "Post Identity",
                Request = SerializationExtensions.SerializeWithoutCasing(request),
                Response = null == response ? string.Empty : SerializationExtensions.SerializeWithoutCasing(response),
                PreviousLinkId = String.Empty
            };
        }

        public HcaMpiNotification BuildLinkIdentityNotification(LinkClientIdentityRequest request, LinkClientIdentityResponse? response, string previousLinkId)
        {
            return new HcaMpiNotification()
            {
                LinkId = response?.Content.LinkId ?? "Link Id not Generated",
                TrackingId = request.TrackingId,
                TimeStamp = DateTime.Now,
                SourceSystemName = request.Content.Source.Name,
                SourceSystemId = request.Content.Source.Id,
                Operation = "Link Identities",
                Request = SerializationExtensions.SerializeWithoutCasing(request),
                Response = null == response ?  string.Empty : SerializationExtensions.SerializeWithoutCasing(response),
                PreviousLinkId = previousLinkId
            };
        }

        public HcaMpiNotification BuildUnLinkIdentityNotification(UnLinkClientIdentityRequest request, UnLinkClientIdentityResponse? response, string previousLinkId)
        {
            return new HcaMpiNotification()
            {
                LinkId = response?.Content.UnlinkedId ?? "Un Link Id not Generated",
                TrackingId = request.TrackingId,
                TimeStamp = DateTime.Now,
                SourceSystemName = request.Content.Source.Name,
                SourceSystemId = request.Content.Source.Id,
                Operation = "Un Link Identities",
                Request = SerializationExtensions.SerializeWithoutCasing(request),
                Response = null == response ? string.Empty : SerializationExtensions.SerializeWithoutCasing(response),
                PreviousLinkId = previousLinkId
            };
        }

        public HcaMpiNotification BuildMergeIdentityNotification(MergeClientIdentityRequest request, MergeClientIdentityResponse? response, string previousLinkId)
        {
            return new HcaMpiNotification()
            {
                LinkId = response?.Content.LinkId ?? "Merge Id not Generated",
                TrackingId = request.TrackingId,
                TimeStamp = DateTime.Now,
                SourceSystemName = request.Content.ToRetireSource.Name,
                SourceSystemId = request.Content.ToRetireSource.Id,
                Operation = "Merge Identities",
                Request = SerializationExtensions.SerializeWithoutCasing(request),
                Response = null == response ? string.Empty : SerializationExtensions.SerializeWithoutCasing(response),
                PreviousLinkId = previousLinkId
            };
        }

        public HcaMpiNotification BuildUnMergeIdentityNotification(UnMergeClientIdentityRequest request, UnMergeClientIdentityResponse? response, string previousLinkId)
        {
            return new HcaMpiNotification()
            {
                LinkId = response?.Content.UnmergedId ?? "Un Merge Id not Generated",
                TrackingId = request.TrackingId,
                TimeStamp = DateTime.Now,
                SourceSystemName = request.Content.UnmergeSource.Name,
                SourceSystemId = request.Content.UnmergeSource.Id,
                Operation = "Un Merge Identities",
                Request = SerializationExtensions.SerializeWithoutCasing(request),
                Response = null == response ? string.Empty : SerializationExtensions.SerializeWithoutCasing(response),
                PreviousLinkId = previousLinkId
            };
        }

        public HcaMpiNotification BuildDemographicSearchNotification(DemographicSearchClientIdentityRequest request, DemographicSearchClientIdentityResponse? response)
        {
            return new HcaMpiNotification()
            {
                LinkId = "Demographic Search",
                TrackingId = request.TrackingId,
                TimeStamp = DateTime.Now,
                SourceSystemName = "Demographic Search",
                SourceSystemId = "Demographic Search",
                Operation = "Demographic Search",
                Request = SerializationExtensions.SerializeWithoutCasing(request),
                Response = null == response ? string.Empty : SerializationExtensions.SerializeWithoutCasing(response),
                PreviousLinkId = String.Empty
            };
        }
    }
}
