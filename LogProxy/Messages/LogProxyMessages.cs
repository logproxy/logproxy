using System.Text.Json.Serialization;

namespace LogProxy.Messages
{
    public class TitleAndText
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
    
    public class EnrichedTitleAndText : TitleAndText
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("receivedAt")]
        public string ReceivedAt { get; set; }
    }
}