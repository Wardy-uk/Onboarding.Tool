using System.Text.Json;
using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Converters;

public class JsonStringEnumConverterWithDefaults : JsonStringEnumConverter
{
    public JsonStringEnumConverterWithDefaults() 
        : base(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    {

    }
}
