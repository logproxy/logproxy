using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LogProxy.Messages
{
    public class Fields
    {
        [JsonPropertyName("id")]
        public string Id {get;set;}
        public string Summary { get; set; }
        public string Message { get; set; }
        [JsonPropertyName("receivedAt")]
        public string ReceivedAt { get; set; }
    }

    public class Record
    {
        [JsonPropertyName("fields")]
        public Fields Fields { get; set; }
    }
    public class EnrichedRecord : Record
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("createdTime")]
        public string CreatedTime { get; set; }
    }
    public class AirTableResponse
    {
        [JsonPropertyName("records")]
        public IEnumerable<EnrichedRecord> Records { get; set; }
        [JsonPropertyName("offset")]
        public string Offset { get; set; }
    }

    public class AirTableRequest
    {
        [JsonPropertyName("records")]
        public IEnumerable<Record> Records { get; set; }
    }
}