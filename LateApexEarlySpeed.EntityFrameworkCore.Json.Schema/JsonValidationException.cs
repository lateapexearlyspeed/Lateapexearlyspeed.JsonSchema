using System;
using LateApexEarlySpeed.Json.Schema.Common;

namespace LateApexEarlySpeed.EntityFrameworkCore.Json.Schema
{
    public class JsonValidationException : Exception
    {
        public ValidationResult DetailedInfo { get; }

        public JsonValidationException(ValidationResult validationResult)
            : base($"Failed to validate json column, {validationResult.ErrorMessage}, failed json location (json pointer): '{validationResult.InstanceLocation}'")
        {
            DetailedInfo = validationResult;
        }
    }
}