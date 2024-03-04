using System;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using Xunit.Sdk;

namespace LateApexEarlySpeed.Xunit.Assertion.Json
{
    public static class JsonAssertion
    {
        public static void Meet(Action<JsonSchemaBuilder> expectedSchemaConfiguration, string actualJson)
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            expectedSchemaConfiguration(jsonSchemaBuilder);
            JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();
            ValidationResult validationResult = jsonValidator.Validate(actualJson);

            if (!validationResult.IsValid)
            {
                throw new JsonAssertException($"{nameof(JsonAssertion)}.{nameof(Meet)}() Failure: {validationResult.ErrorMessage}, location (in json pointer format): '{validationResult.InstanceLocation}'");
            }
        }

        public static void Equivalent(string expectedJson, string actualJson)
        {
            var jsonSchemaBuilder = new JsonSchemaBuilder();
            jsonSchemaBuilder.Equivalent(expectedJson);
            JsonValidator jsonValidator = jsonSchemaBuilder.BuildValidator();
            ValidationResult validationResult = jsonValidator.Validate(actualJson);

            if (!validationResult.IsValid)
            {
                throw new JsonAssertException($"{nameof(JsonAssertion)}.{nameof(Equivalent)}() Failure: {validationResult.ErrorMessage}, location (in json pointer format): '{validationResult.InstanceLocation}'");
            }
        }
    }

    public class JsonAssertException : XunitException
    {
        public JsonAssertException(string? userMessage) : base(userMessage)
        {
        }
    }
}
