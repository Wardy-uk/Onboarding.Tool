using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class Address
{
    // [JsonIgnore]
    public int Id { get; set; }

    public required string Address1 { get; set; }
    
    public string? Address2 { get; set; }

    public string? Address3 { get; set; }

    public required string Town { get; set; }

    public required string PostCode1 { get; set; }

    public required string PostCode2 { get; set; }
}
