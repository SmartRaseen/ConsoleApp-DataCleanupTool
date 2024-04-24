using Newtonsoft.Json.Linq;
using DataCleanupTool.Model;
using System.Collections.Generic;
using Ext = DataCleanupTool.ValidatorExtension;
using DataCleanupTool.Schema;
using DataCleanupTool.Utils;
using System;

namespace DataCleanupTool
{
    public class DataValidatorExtension
    {
        public static List<TotalDataValidator> DuplicateProductIdValidators { get; set; } = new List<TotalDataValidator>();
        private const string outputDirectory = @"Output";

        public static void ValidateDuplicateProductIds(Dictionary<string, JObject> jsonContents)
        {
            foreach (var kvp in jsonContents)
            {
                List<KeyValuePair<string, JObject>> itemsToBeModifiedList = new List<KeyValuePair<string, JObject>>();

                // Parse JSON content
                JObject jsonObject = kvp.Value;

                // Extract itemsForDeletion and itemsToBeModified
                JArray itemsForDeletion = (JArray)jsonObject["itemsForDeletion"];

                foreach (var jsonContent in jsonContents)
                {
                    // Parse JSON content
                    JObject jObject = jsonContent.Value;

                    // Extract itemsToBeModified for the current file
                    JObject itemsToBeModified = (JObject)jObject["itemsToBeModified"];

                    // Add itemsToBeModified to the list along with the filename
                    itemsToBeModifiedList.Add(new KeyValuePair<string, JObject>(jsonContent.Key, itemsToBeModified));
                }

                // Check for duplicates between itemsForDeletion and itemsToBeModified
                foreach (var deletionId in itemsForDeletion)
                {
                    string id = deletionId.ToString();

                    // Iterate through each itemsToBeModified list
                    foreach (var itemsToBeModifiedObj in itemsToBeModifiedList)
                    {
                        // Check if the id exists in the current itemsToBeModified list
                        if (itemsToBeModifiedObj.Value.ContainsKey(id))
                        {
                            // If the id is present in any list of itemsToBeModified, set the flag to true
                            var totaldatavalidator = new TotalDataValidator()
                            {
                                FileName = kvp.Key,
                                ProductName = id,
                                Key = "itemsForDeletion",
                                Value = $"This '{id}' product have duplicates in 'itemsToBeModified' fileName: '{itemsToBeModifiedObj.Key}'",
                                Validation = "Error"
                            };

                            // Validate itemsForDeletion duplicates in itemsToBeModified
                            DuplicateProductIdValidators.Add(totaldatavalidator);

                            break; // Exit inner loop since duplicate found
                        }
                    }
                }
            }

            // Create directory for duplicate productIds
            Ext.CreateOutputFolder(outputDirectory);
            var duplicateProductIdsOutput = $"{outputDirectory}/DuplicateProductIds/DuplicateProductIdsDetail_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";

            // Writing the dupicate product Ids csv file
            if (DuplicateProductIdValidators.Count >= 1)
            {
                Console.WriteLine($"\nWriting summary output to: {duplicateProductIdsOutput}");
                FileUtils.WriteCsvFile(duplicateProductIdsOutput, DuplicateProductIdValidators, new TotalDataValidatorMap());
            }
        }
    }
}
