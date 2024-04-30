using Newtonsoft.Json.Linq;
using DataCleanupTool.Model;
using System.Collections.Generic;
using Ext = DataCleanupTool.ValidatorExtension;
using DataCleanupTool.Schema;
using DataCleanupTool.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataCleanupTool
{
    public class DataValidatorExtension
    {
        public static List<DetailedDataValidator> DuplicateProductIdValidators { get; set; } = new List<DetailedDataValidator>();
        private const string outputDirectory = @"Output";

        public static void ValidateDuplicateProductIds(
            Dictionary<string, JObject> jsonContents, 
            List<DetailedDataValidator> ConsolidatedErrors)
        {
            foreach (var kvp in jsonContents)
            {
                List<KeyValuePair<string, JArray>> itemsForDeletionList = new List<KeyValuePair<string, JArray>>();
                List<KeyValuePair<string, JObject>> itemsToBeModifiedList = new List<KeyValuePair<string, JObject>>();

                // Parse JSON content
                JObject jsonObject = kvp.Value;

                // Extract itemsForDeletion for the current input
                JArray itemsForDeletion = (JArray)jsonObject["itemsForDeletion"];

                // Extract itemsToBeModified for the current input
                JObject itemToBeModified = (JObject)jsonObject["itemsToBeModified"];

                foreach (var jsonContent in jsonContents)
                {
                    // Parse JSON content
                    JObject jObject = jsonContent.Value;

                    // Extract itemsForDeletion from all the input file
                    JArray itemForDeletion = (JArray)jObject["itemsForDeletion"];

                    // Extract itemsToBeModified from all the input file
                    JObject itemsToBeModified = (JObject)jObject["itemsToBeModified"];

                    // Add itemsForDeletion to the list along with the filename
                    itemsForDeletionList.Add(new KeyValuePair<string, JArray>(jsonContent.Key, itemForDeletion));

                    // Add itemsToBeModified to the list along with the filename
                    itemsToBeModifiedList.Add(new KeyValuePair<string, JObject>(jsonContent.Key, itemsToBeModified));
                }

                // Check for duplicates productIds in itemsForDeletion
                foreach (var deletionId in itemsForDeletion)
                {
                    string id = deletionId.ToString();

                    // Iterate through each itemForDeletionList
                    foreach (var itemForDeletionObj in itemsForDeletionList)
                    {
                        // Check if the id exists in the current itemsForDeletion 
                        if ((kvp.Key == itemForDeletionObj.Key && itemForDeletionObj.Value.Count(val => val.ToString() == id) > 1) || 
                            (kvp.Key != itemForDeletionObj.Key && itemForDeletionObj.Value.Any(val => val.ToString() == id)))
                        {
                            var totaldatavalidator = new DetailedDataValidator()
                            {
                                FileName = kvp.Key,
                                ProductName = id,
                                Key = "itemsForDeletion",
                                Value = $"This '{id}' product have duplicates in 'itemsForDeletion' fileName: '{itemForDeletionObj.Key}'",
                                Validation = "Error"
                            };

                            // Adding itemsForDeletion duplicates
                            DuplicateProductIdValidators.Add(totaldatavalidator);

                            // Data validations
                            ConsolidatedErrors.Add(totaldatavalidator);
                        }
                    }
                }

                // Check for duplicates productIds in itemsToBeModified
                foreach (var modifiedId in itemToBeModified)
                {
                    string id = modifiedId.Key.ToString();

                    // Iterate through each itemsToBeModifiedList
                    foreach (var itemsToBeModifiedObj in itemsToBeModifiedList)
                    {
                        // Check if the id exists in the current itemToBeModified
                        if ((kvp.Key == itemsToBeModifiedObj.Key && itemsToBeModifiedObj.Value.Properties().Count(val => val.Name.ToString() == id) > 1) ||
                            (kvp.Key != itemsToBeModifiedObj.Key && itemsToBeModifiedObj.Value.Properties().Any(val => val.Name.ToString() == id)))
                        {
                            var totaldatavalidator = new DetailedDataValidator()
                            {
                                FileName = kvp.Key,
                                ProductName = id,
                                Key = "itemsToBeModified",
                                Value = $"This '{id}' product have duplicates in 'itemsToBeModified' fileName: '{itemsToBeModifiedObj.Key}'",
                                Validation = "Error"
                            };

                            // Adding itemsForDeletion duplicates
                            DuplicateProductIdValidators.Add(totaldatavalidator);

                            // Data validations
                            ConsolidatedErrors.Add(totaldatavalidator);
                        }
                    }
                }


                // Check for duplicates between itemsForDeletion and itemsToBeModified
                foreach (var deletionId in itemsForDeletion)
                {
                    string id = deletionId.ToString();

                    // Iterate through each itemsToBeModified list
                    foreach (var itemsToBeModifiedObj in itemsToBeModifiedList)
                    {
                        // Check if the id exists in the current itemsToBeModified
                        if (itemsToBeModifiedObj.Value.ContainsKey(id))
                        {
                            var totaldatavalidator = new DetailedDataValidator()
                            {
                                FileName = kvp.Key,
                                ProductName = id,
                                Key = "itemsForDeletion",
                                Value = $"This '{id}' product have duplicates in 'itemsToBeModified' fileName: '{itemsToBeModifiedObj.Key}'",
                                Validation = "Error"
                            };

                            // Validate itemsForDeletion duplicates in itemsToBeModified
                            DuplicateProductIdValidators.Add(totaldatavalidator);

                            // Data validations
                            ConsolidatedErrors.Add(totaldatavalidator);
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
                FileUtils.WriteCsvFile(duplicateProductIdsOutput, DuplicateProductIdValidators, new DetailedDataValidatorMap());
            }
        }


        public static void GetIdsFromItemsToBeModified(string fileName, string jsonContent, List<DetailedDataValidator> ConsolidatedErrors)
        {
            List<string> ids = new List<string>();

            // Find the start index of the "itemsToBeModified" section
            int startIndex = jsonContent.IndexOf("\"itemsToBeModified\"");

            // Check if the section exists
            if (startIndex != -1)
            {
                // Find the start and end indices of the section
                int startBraceIndex = jsonContent.IndexOf('{', startIndex);
                int endBraceIndex = Ext.FindClosingBraceIndex(jsonContent, startBraceIndex);

                // Extract the section content as a substring
                string sectionContent = jsonContent.Substring(startBraceIndex + 1, endBraceIndex - startBraceIndex - 1);

                // Use regular expression to find all IDs in the section content
                string pattern = @"\""([A-Z0-9]{10})\"":\s*{";

                // Create a Regex object
                Regex regex = new Regex(pattern);

                // Find all matches in the input string
                MatchCollection matches = regex.Matches(sectionContent);

                // Add the IDs to the list
                foreach (Match match in matches)
                {
                    ids.Add(match.Groups[1].Value);
                }
            }

            List<string> duplicateIds = Ext.GetDuplicateIds(ids);

            foreach (string duplicateId in duplicateIds)
            {
                var totaldatavalidator = new DetailedDataValidator()
                {
                    FileName = fileName,
                    ProductName = duplicateId,
                    Key = "itemsToBeModified",
                    Value = $"This '{duplicateId}' product have duplicates in 'itemsToBeModified' fileName: '{fileName}'",
                    Validation = "Error"
                };

                DuplicateProductIdValidators.Add(totaldatavalidator);
                ConsolidatedErrors.Add(totaldatavalidator);
            }
        }
    }
}
