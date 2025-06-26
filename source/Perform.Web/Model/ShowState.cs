using System.Text.Json.Serialization;

namespace Perform.Web.Model;

public class ShowState
{
    public string? Show { get; set; }

    public string? Description { get; set; }

    public int Song { get; set; } = -1;

    public IEnumerable<object> Songs { get; set; } = Array.Empty<object>();

    public bool Active { get; set; }

    public Dictionary<int, bool?> Stomps { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? AvailableShows { get; set; }
}