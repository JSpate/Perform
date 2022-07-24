using System.Text.Json.Serialization;

namespace Perform.Model
{
    public class Buttons
    {
        [JsonConstructor]
        public Buttons(double longPress, IList<int>? keys)
        {
            LongPress = longPress;
            Keys = keys ?? new List<int>();
        }

        [JsonPropertyName("longPress")]
        public double LongPress { get; }

        [JsonPropertyName("keys")]
        public IList<int> Keys { get; }
    }
}
