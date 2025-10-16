using HCA.Models.Request;
using Newtonsoft.Json.Linq;

namespace HCA.Infrastructure.JObjectHelper
{
    public class VeratoHelper
    {
        public static JObject ConvertPropertyNames(JObject inputObject)
        {
            JObject convertedObject = new JObject();

            foreach (var property in inputObject.Properties())
            {
                string oldName = property.Name;
                string newName = ConvertPropertyName(oldName);
                JToken value = property.Value;

                if (value.Type == JTokenType.Object)
                {
                    value = ConvertPropertyNames((JObject)value); // Recursively convert nested objects
                }
                else if (value.Type == JTokenType.Array)
                {
                    var array = new JArray();
                    foreach (var item in value)
                    {
                        if (item.Type == JTokenType.Object)
                        {
                            array.Add(ConvertPropertyNames((JObject)item)); // Recursively convert objects in array
                        }
                        else
                        {
                            array.Add(item);
                        }
                    }
                    value = array;
                }

                convertedObject.Add(newName, value);
            }

            return convertedObject;
        }

        public static string ConvertPropertyName(string oldName)
        {
            return oldName.Substring(0, 1).ToLower() + oldName.Substring(1);       }

        public static JObject MergedObjects(IEnumerable<ClientIdentityRequest> clientIdentities)
        {
            JObject mergedObject = new JObject();

            foreach (var clientIdentity in clientIdentities)
            {
                if (clientIdentity.CustomJson == null)
                    continue;

                var jsonObject = JObject.Parse(clientIdentity.CustomJson);

                foreach (JProperty property in jsonObject.Properties())
                {
                    string propertyName = property.Name;

                    if (mergedObject[propertyName] == null)
                    {
                        mergedObject.Add(propertyName, new JArray());
                    }

                    (mergedObject[propertyName] as JArray).Add(property.Value);
                }
            }
            return mergedObject;
        }
    }
}
