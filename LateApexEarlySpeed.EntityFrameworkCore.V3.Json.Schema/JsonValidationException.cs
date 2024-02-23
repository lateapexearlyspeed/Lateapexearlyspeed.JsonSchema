using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema
{
    public class JsonValidationException : Exception
    {
        public ValidationResult DetailedInfo { get; }

        public JsonValidationException(string propertyName, ValidationResult validationResult)
            : base($"Failed to validate json property: '{propertyName}'. Failed json location (json pointer format): '{validationResult.InstanceLocation}', reason: {validationResult.ErrorMessage}.")
        {
            DetailedInfo = validationResult;
        }
    }
}