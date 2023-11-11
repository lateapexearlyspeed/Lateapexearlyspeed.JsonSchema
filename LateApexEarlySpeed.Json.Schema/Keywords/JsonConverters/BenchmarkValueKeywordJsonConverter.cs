using System.Text.Json;
using System.Text.Json.Serialization;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.Keywords.interfaces;

namespace LateApexEarlySpeed.Json.Schema.Keywords.JsonConverters;

internal class BenchmarkValueKeywordJsonConverter<TBenchmarkValueKeyword> : JsonConverter<TBenchmarkValueKeyword>
    where TBenchmarkValueKeyword : IBenchmarkValueKeyword, new()
{
    public override TBenchmarkValueKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!reader.TryGetUInt32(out uint size))
        {
            throw ThrowHelper.CreateKeywordHasInvalidNonNegativeIntegerJsonException(typeToConvert);
        }

        return new TBenchmarkValueKeyword { BenchmarkValue = size };
    }

    public override void Write(Utf8JsonWriter writer, TBenchmarkValueKeyword value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}