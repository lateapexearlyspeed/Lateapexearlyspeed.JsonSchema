using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema
{
    public class JsonStringValueConverter : ValueConverter<string, string>
    {
        public JsonStringValueConverter(string propertyName, JsonValidator jsonValidator) : base(model => ConvertToJson(model, jsonValidator, propertyName), provider => ConvertToModel(provider))
        {
        }

        private static string ConvertToModel(string provider)
        {
            return provider;
        }

        private static string ConvertToJson(string model, JsonValidator jsonValidator, string propertyName)
        {
            ValidationResult result = jsonValidator.Validate(model);
            if (!result.IsValid)
            {
                throw new JsonValidationException(propertyName, result);
            }

            return model;
        }
    }
}
