using System;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using Xunit.Sdk;

namespace LateApexEarlySpeed.Xunit.Assertion.Json
{
    /// <summary>
    /// Class to make assertion about json data
    /// </summary>
    public static class JsonAssertion
    {
        /// <summary>
        /// Assert that <paramref name="actualJson"/> should meet requirement specified from <paramref name="expectedSchemaConfiguration"/>
        /// </summary>
        /// <param name="expectedSchemaConfiguration">Json data requirement</param>
        /// <param name="actualJson">actual json text</param>
        /// <exception cref="JsonAssertException">If assertion fails, will throw and report error reason and failed json location</exception>
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

        /// <summary>
        /// Assert that <paramref name="actualJson"/> should be json-level equivalent to <paramref name="expectedJson"/>
        /// </summary>
        /// <param name="expectedJson">Expected json structure</param>
        /// <param name="actualJson">Actual json data</param>
        /// <exception cref="JsonAssertException">If assertion fails, will throw and report error reason and failed json location</exception>
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
