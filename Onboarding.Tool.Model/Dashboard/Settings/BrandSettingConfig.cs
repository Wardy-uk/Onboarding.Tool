using Onboarding.Tool.Model.Converters;
using Onboarding.Tool.Model.Enums;
using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Dashboard.Settings;

public class BrandSettingConfig
{
    public required string Key { get; set; }

    public required string Label { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverterWithDefaults))]
    public required BrandSettingConfigTypes Type { get; set; }

    public required bool Required { get; set; }
}
