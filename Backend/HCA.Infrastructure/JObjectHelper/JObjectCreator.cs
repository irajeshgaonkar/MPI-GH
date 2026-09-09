using Newtonsoft.Json.Linq;

namespace HCA.Infrastructure.JObjectHelper
{
    public interface IJObjectCreator
    {
        JObject GetJObject(string jsonString, char hieararchySplitted = '$');
    }

    public class JObjectCreator : IJObjectCreator
    {
        public JObject GetJObject(string jsonString, char hieararchySplitted = '$')
        {
            JObject jsonObject = JObject.Parse(jsonString);

            foreach (var property in jsonObject.Properties().ToList())
            {
                var nestedProperties = property.Name.Split(hieararchySplitted);
                var nestedObject = new JObject();
                JObject currentObject = jsonObject;

                for (int i = 0; i < nestedProperties.Length - 1; i++)
                {
                    var nestedProperty = nestedProperties[i];
                    if (currentObject?.Property(nestedProperty) == null)
                    {
                        var newObject = new JObject();
                        currentObject?.Add(nestedProperty, newObject);
                        currentObject = newObject;
                    }
                    else
                    {
                        currentObject = currentObject[nestedProperty] as JObject
                                        ?? throw new InvalidOperationException($"Property '{nestedProperty}' is not a JObject.");
                    }
                }

                property.Remove();
                currentObject?.Add(nestedProperties[^1], property.Value);
            }

            return jsonObject;
        }
    }
}
