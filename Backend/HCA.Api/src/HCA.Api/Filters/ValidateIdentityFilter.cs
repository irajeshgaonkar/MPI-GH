using HCA.Api.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HCA.Api.Filters {
    public class ValidateIdentityFilter( IAppLogger logger ) : IAsyncActionFilter {
        private readonly IAppLogger _logger = logger;
        private static List<string> validationMessages;
        private static JObject jsonObject;

        private static readonly Dictionary<string, int> basicAttributesToLengths = new()
        {
            ["SourceSystem"] = 100,
            ["Agency"] = 100,
            ["protectedPopulation.ProtectedPopulationFlag"] = 10,
            ["protectedPopulation.ProtectedPopulationTypes"] = 100,
            ["content.identity.linkId"] = 1024,
            ["content.identity.sources.name"] = 100,
            ["content.identity.sources.id"] = 100,
            ["content.identity.names.first"] = 255,
            ["content.identity.names.middle"] = 255,
            ["content.identity.names.last"] = 255,
            ["content.identity.names.suffix"] = 255,
            ["content.identity.emails"] = 255,
            ["content.identity.datesOfBirth"] = 50,
            ["content.identity.genders"] = 10,
            ["content.identity.ssns"] = 12,
            ["content.identity.addresses.line1"] = 255,
            ["content.identity.addresses.line2"] = 255,
            ["content.identity.addresses.line3"] = 255,
            ["content.identity.addresses.city"] = 100,
            ["content.identity.addresses.state"] = 40,
            ["content.identity.addresses.postalCode"] = 10,
            ["content.identity.addresses.zipFour"] = 4,
            ["content.identity.phoneNumbers.number"] = 20
        };

        public async Task OnActionExecutionAsync( ActionExecutingContext context, ActionExecutionDelegate next ) {
            validationMessages = [];
            string? trackingId = string.Empty;

            try {
                var requestBody = context.ActionArguments.FirstOrDefault();

                var resultBody = JsonSerializer.Serialize(requestBody.Value);

                jsonObject = JObject.Parse( resultBody );

                if( jsonObject == null ) {
                    await next();
                    return;
                }

                CheckAllBasicAttributes();

                if( validationMessages.Count > 0 ) {
                    trackingId = jsonObject?["TrackingId"]?.ToString();
                    var message = $"One or more validation errors occurred. {string.Join('|', validationMessages)}";

                    var exceptionCustomProperties = new ExceptionCustomProperties
                    {
                        User = context.HttpContext.GetCurrentUser(),
                        Role = context.HttpContext.GetUserRoles(),
                        FunctionName = nameof(ValidateIdentityFilter),
                        ErrorMessage = message,
                        ErrorCode = "400"
                    };
                    var errorLogItem = new LogItem()
                    {
                        Name = $"{HCA.Models.Logging.Constants.LogPrefix_API}-{nameof(ValidateIdentityFilter)}-Failed",
                        TrackingId = trackingId,
                        Layer = ServiceLayer.API.ToString(),
                        ExceptionCustomProperties = exceptionCustomProperties
                    };

                    _logger.LogError( new BadHttpRequestException( message ), JsonSerializer.Serialize( errorLogItem ) );

                    context.Result = BuildOkObjectResultWith400Error( message, trackingId );
                    return;
                }

                await next();
            }
            catch( Exception ex ) {
                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = context.HttpContext.GetCurrentUser(),
                    Role = context.HttpContext.GetUserRoles(),
                    FunctionName = nameof(ValidateIdentityFilter),
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{HCA.Models.Logging.Constants.LogPrefix_API}-{nameof(ValidateIdentityFilter)}-Failed",
                    TrackingId = trackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogError( ex, JsonSerializer.Serialize( errorLogItem ) );
                context.Result = BuildOkObjectResultWith400Error( "Identity property validation failed. Unknown error: " + ex.Message );
                throw;
            }
        }

        private static void CheckAllBasicAttributes() {
            foreach( var basicAttribute in basicAttributesToLengths ) {
                List<JToken> attributeValues = GetIdentityAttributeValues(jsonObject, basicAttribute.Key);
                foreach( var attributeValue in attributeValues ) {
                    if( attributeValue.Type == JTokenType.Array ) {
                        foreach( var childItem in attributeValue.ToList() ) {

                            ValidateAttribute( basicAttribute.Key, childItem.ToString(), basicAttribute.Value );
                        }
                    }
                    else {

                        ValidateAttribute( basicAttribute.Key, attributeValue.ToString(), basicAttribute.Value );
                    }
                }
            }
        }

        /// <summary>
        /// Custom validation for attributes
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        private static void CheckCustomValidation( string? attributeName, string? attributeValue ) {
            switch( attributeName ) {
                case "content.identity.sources.name":
                case "content.identity.sources.id":
                    ValidateNullOrEmpty( attributeName, attributeValue );
                    break;
                //TODO: Enable custom validation for Date/Email/PhoneNumber later once the basic Validations are stabilized.
                //case "content.identity.datesOfBirth":
                //    ValidateAttributeDate( attributeName, attributeValue );
                //    break;

                //case "content.identity.emails":
                //    var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                //    ValidateAttributePattern( attributeName, attributeValue, emailPattern );
                //    break;

                //case "content.identity.phoneNumbers.number":
                //    var phonePattern = @"^\+?(\d{1,3})?[-.\s]?\(?\d{1,4}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}([-\s]?((ext|x)[-\s]?\d{1,5}))?$";
                //    ValidateAttributePattern( attributeName, attributeValue, phonePattern );
                //    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Validate against max length
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <param name="maxLength"></param>
        private static void ValidateAttribute( string? attributeName, string? attributeValue, int maxLength ) {
            if( !string.IsNullOrEmpty( attributeValue ) && attributeValue.Length > maxLength ) {
                validationMessages.Add( $"{attributeName} '{attributeValue}' must be {maxLength} characters or fewer." );
            }

            CheckCustomValidation(attributeName, attributeValue);
        }

        /// <summary>
        /// Validate against Null or Empty
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        private static void ValidateNullOrEmpty( string? attributeName, string? attributeValue ) {
            if( string.IsNullOrEmpty( attributeValue ) ) {
                validationMessages.Add( $"{attributeName} cannot be Null or Empty." );
            }
        }

        /// <summary>
        /// Validate against a pattern
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <param name="patternToMatch"></param>
        private static void ValidateAttributePattern( string? attributeName, string? attributeValue, string patternToMatch ) {
            if( !string.IsNullOrEmpty( attributeName ) && !string.IsNullOrEmpty( attributeValue ) && !string.IsNullOrEmpty( patternToMatch ) ) {
                if( !Regex.IsMatch( attributeValue, patternToMatch ) ) {
                    validationMessages.Add( $"{attributeName} '{attributeValue}' is not in a valid format." );
                }
            }
        }

        /// <summary>
        /// Validate against DateTime
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        private static void ValidateAttributeDate( string? attributeName, string? attributeValue ) {
            if( !DateTime.TryParse( attributeValue, out _ ) ) {
                validationMessages.Add( $"{attributeName} '{attributeValue}' is not a valid Date." );
            }
        }

        /// <summary>
        /// Traverse the JSON root, given a specific attribute path, and extract the values for the attribute,
        /// even if it's nested deeply within arrays or objects.
        /// </summary>
        /// <param name="root"> The root JSON token from which to start the search</param>
        /// <param name="attributePath"> '.' delimited path of the attribute whose values are to be extracted</param>
        /// <returns> A list of JToken views that match the attribute path </returns>
        private static List<JToken> GetIdentityAttributeValues( JToken root, string attributePath ) {
            if( root == null ) {
                return [];
            }

            //split the attribute path to get the hierarchy
            var paths = attributePath.Split('.');

            //Call the recursive function to get the attribute values, starting from first in hierarchy
            return GetIdentityAttributeValues( root, paths, 0 );

        }

        /// <summary>
        /// Get Identity attribute values recursively. This ensures that all nested attributes,
        /// whether arrays or objects, are properly traversed and values are collected.
        /// </summary>
        /// <param name="root"> The current JSON token being examined</param>
        /// <param name="paths"> Attribute path components</param>
        /// <param name="index"> The current index of component in path components</param>
        /// <returns> A list of JToken that match the attribute path</returns>
        private static List<JToken> GetIdentityAttributeValues( JToken? root, string[] paths, int index ) {
            var values = new List<JToken>();
            if( root == null ) {
                return values;
            }

            //If we've traversed all the way to the end of path, we add the current
            //root value to the result, provided it's not null and return the current results.
            //Completion of recursion for given attribute path.
            if( index == paths.Length ) {
                if( root != null ) {
                    values.Add( root );
                }

                return values;
            }

            //Grab the current segment of path being processed.
            var currentPath = paths[index];

            //If root is an Array, iterate over each item in it
            if( root != null && root.Type == JTokenType.Array ) {
                foreach( var item in (JArray)root ) {
                    values.AddRange( GetIdentityAttributeValues( item, paths, index ) );
                }
                return values;
            }
            //If root is an Object, move root to the next token in JSON
            //Recursively call for the next path segment
            if( currentPath != null && root?.Type != JTokenType.Null ) {
                root = root?[currentPath];
            }
            if( root != null ) {
                values.AddRange( GetIdentityAttributeValues( root, paths, index + 1 ) );
            }

            return values;
        }

        private static OkObjectResult BuildOkObjectResultWith400Error( string message, string? trackingId = "" )
            => new( new { errorCode = "400", Message = message, Success = false, TrackingId = trackingId } );
    }
}
