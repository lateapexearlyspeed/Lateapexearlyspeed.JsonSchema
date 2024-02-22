using System;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LateApexEarlySpeed.EntityFrameworkCore.Json.Schema
{
    public static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<string> HasJsonValidation(this PropertyBuilder<string> propertyBuilder, string jsonSchema)
        {
            JsonValidator jsonValidator = new JsonValidator(jsonSchema);

            ValueConverter jsonValueConverter = new JsonStringValueConverter(jsonValidator);

            return propertyBuilder.HasConversion(jsonValueConverter);
        }

        public static PropertyBuilder<string> HasJsonValidation(this PropertyBuilder<string> propertyBuilder, Action<JsonSchemaBuilder> configureSchema)
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            configureSchema(jsonSchemaBuilder);
            JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();

            ValueConverter jsonValueConverter = new JsonStringValueConverter(jsonValidator);

            return propertyBuilder.HasConversion(jsonValueConverter);
        }
    }
}