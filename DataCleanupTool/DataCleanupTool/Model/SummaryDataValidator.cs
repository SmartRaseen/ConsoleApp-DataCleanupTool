﻿using System.Text.Json.Serialization;

namespace DataCleanupTool.Model
{
    public class SummaryDataValidator
    {
        [JsonPropertyName("FileName")]
        public string FileName { get; set; }
        [JsonPropertyName("ProductName")]
        public string ProductName { get; set; }
        [JsonPropertyName("Validation")]
        public string Validation { get; set; }
    }
}
