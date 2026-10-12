using Newtonsoft.Json.Linq;

namespace HCA.Core.Services
{
    public static class IdentityLinkIdSelector
    {
        public static string? Select(string? searchContentJson, string caller, IEnumerable<string> allowedLinkIdSystems)
        {
            if (string.IsNullOrWhiteSpace(searchContentJson) || string.IsNullOrWhiteSpace(caller))
            {
                return null;
            }

            if (JToken.Parse(searchContentJson) is not JObject content || content["searchResults"] is not JArray searchResults)
            {
                return null;
            }

            var allowed = new HashSet<string>(
                allowedLinkIdSystems.Where(name => !string.IsNullOrWhiteSpace(name)),
                StringComparer.OrdinalIgnoreCase);
            var linkIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var result in searchResults)
            {
                if (!IsVisible(result, caller, allowed))
                {
                    continue;
                }

                var linkId = result["linkId"]?.ToString();
                if (!string.IsNullOrWhiteSpace(linkId))
                {
                    linkIds.Add(linkId);
                }
            }

            return linkIds.Count == 1 ? linkIds.First() : null;
        }

        private static bool IsVisible(JToken result, string caller, HashSet<string> allowed)
        {
            if (result["identityGroupedBySource"] is not JArray groups)
            {
                return false;
            }

            foreach (var group in groups)
            {
                var name = group["source"]?["name"]?.ToString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (string.Equals(name, caller, StringComparison.OrdinalIgnoreCase) || allowed.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
