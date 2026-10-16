using Newtonsoft.Json.Linq;

namespace HCA.Core.Services
{
    public static class IdentityLinkIdSelector
    {
        /// <summary>
        /// Picks the MPI identity link id from a demographics query response.
        /// </summary>
        /// <param name="queryContentJson">JSON content from demographics query, including linkId and identityGroupedBySource.</param>
        /// <param name="callingSourceSystem">Source system that made the identity link id request.</param>
        /// <param name="allowedLinkIdSystems">Other source system names this caller may see at the LinkId share level.</param>
        /// <returns>
        /// The linkId string when at least one source on the identity is the calling source system
        /// or an allowed LinkId share; otherwise null.
        /// </returns>
        public static string? Select(string? queryContentJson, string callingSourceSystem, IEnumerable<string> allowedLinkIdSystems)
        {
            if (string.IsNullOrWhiteSpace(queryContentJson) || string.IsNullOrWhiteSpace(callingSourceSystem))
            {
                return null;
            }

            if (JToken.Parse(queryContentJson) is not JObject content)
            {
                return null;
            }

            var linkId = content["linkId"]?.ToString();
            if (string.IsNullOrWhiteSpace(linkId))
            {
                return null;
            }

            if (content["identityGroupedBySource"] is not JArray groups)
            {
                return null;
            }

            var allowed = new HashSet<string>(
                allowedLinkIdSystems.Where(name => !string.IsNullOrWhiteSpace(name)),
                StringComparer.OrdinalIgnoreCase);

            var isVisible = groups.Any(group =>
            {
                var name = group["source"]?["name"]?.ToString();
                return !string.IsNullOrWhiteSpace(name)
                    && (string.Equals(name, callingSourceSystem, StringComparison.OrdinalIgnoreCase)
                        || allowed.Contains(name));
            });

            return isVisible ? linkId : null;
        }
    }
}
