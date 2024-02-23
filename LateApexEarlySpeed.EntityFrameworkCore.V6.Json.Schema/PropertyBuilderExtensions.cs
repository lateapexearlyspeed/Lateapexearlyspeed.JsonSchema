using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema
{
    public static class PropertyBuilderExtensions
    {
        /// <summary>
        /// Use <paramref name="jsonSchema"/> as standard json schema to validate current json property
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <param name="jsonSchema">Support Json schema standard - 2020.12</param>
        /// <returns></returns>
        public static PropertyBuilder<string> HasJsonValidation(this PropertyBuilder<string> propertyBuilder, string jsonSchema)
        {
            JsonValidator jsonValidator = new JsonValidator(jsonSchema);

            ValueConverter jsonValueConverter = new JsonStringValueConverter(propertyBuilder.Metadata.Name, jsonValidator);

            return propertyBuilder.HasConversion(jsonValueConverter);
        }

        /// <summary>
        /// Configure <see cref="JsonSchemaBuilder"/> to construct schema to validate current json property
        /// </summary>
        /// <param name="propertyBuilder"></param>
        /// <param name="configureSchema">construct schema metadata by specifying constraint requirement</param>
        /// <returns></returns>
        public static PropertyBuilder<string> HasJsonValidation(this PropertyBuilder<string> propertyBuilder, Action<JsonSchemaBuilder> configureSchema)
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            configureSchema(jsonSchemaBuilder);
            JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

            ValueConverter jsonValueConverter = new JsonStringValueConverter(propertyBuilder.Metadata.Name, jsonValidator);

            return propertyBuilder.HasConversion(jsonValueConverter);
        }
    }
}