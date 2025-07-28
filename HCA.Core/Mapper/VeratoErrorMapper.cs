namespace HCA.Core.Mapper
{
    public class VeratoErrorMapper
    {
        private static readonly Dictionary<Predicate<string>, string> ErrorMappings = new()
    {
        { msg => msg.Contains("Error during ingest: Overlay Error Detected", StringComparison.OrdinalIgnoreCase),
            "The demographic data posted is very different from the current demographics for the same Source + Native ID, indicating an overlay condition." }

        // Add more error patterns as needed
    };

        public static string MapErrorMessage(string originalMessage)
        {
            foreach (var mapping in ErrorMappings)
            {
                if (mapping.Key(originalMessage))
                    return mapping.Value;
            }

            return originalMessage;
        }
    }
}
