using System.Text.Json.Serialization;

namespace DataCleanupTool.Model
{
    public class TotalDataValidator
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; }

        [JsonPropertyName("ProductName")]
        public string ProductName { get; set; }

        [JsonPropertyName("Key")]
        public string Key { get; set; }

        [JsonPropertyName("Value")]
        public string Value { get; set; }

        [JsonPropertyName("Validation")]
        public string Validation { get; set; }
    }
}
