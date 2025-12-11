using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class Image
{
    public int Id { get; set; }

    public required ImageType Type { get; set; }

    public required string Name { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public required byte[] Data { get; set; }

    public int? CardXOverride { get; set; }

    public int? CardYOverride { get; set; }

    public int? CardWidthOverride { get; set; }

    public int? CardHeightOverride { get; set; }

    [JsonIgnore]
    public int InstanceId { get; set; }

    [JsonIgnore]
    public Instance? Instance { get; set; }
}
