using System.Text.Json.Serialization;

namespace DataCleanupTool.Model
{
    public class Report
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; }

        [JsonPropertyName("ProductId")]
        public string ProductId { get; set; }

        [JsonPropertyName("gender")]
        public string gender { get; set; }

        [JsonPropertyName("ageGroup")]
        public string ageGroup { get; set; }

        [JsonPropertyName("productType")]
        public string productType { get; set; }

        [JsonPropertyName("imageUrlIndexes")]
        public string imageUrlIndexes { get; set; }

        [JsonPropertyName("Action")]
        public string Action { get; set; }
    }
}
