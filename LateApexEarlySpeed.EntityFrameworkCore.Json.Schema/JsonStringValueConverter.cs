using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LateApexEarlySpeed.EntityFrameworkCore.Json.Schema
{
    public class JsonStringValueConverter : ValueConverter<string, string>
    {
        public JsonStringValueConverter(JsonValidator jsonValidator) : base(model => ConvertToJson(model, jsonValidator), provider => ConvertToModel(provider))
        {
        }

        private static string ConvertToModel(string provider)
        {
            return provider;
        }

        private static string ConvertToJson(string model, JsonValidator jsonValidator)
        {
            ValidationResult result = jsonValidator.Validate(model);
            if (!result.IsValid)
            {
                throw new JsonValidationException(result);
            }

            return model;
        }
    }
}
