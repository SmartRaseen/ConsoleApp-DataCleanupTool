using System;
using System.IO;
using Newtonsoft.Json.Linq;
using DataCleanupTool.Model;
using DataCleanupTool.Utils;
using DataCleanupTool.Schema;
using System.Collections.Generic;
using Ext = DataCleanupTool.ValidatorExtension;
using DataExt = DataCleanupTool.DataValidatorExtension;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace DataCleanupTool
{
    internal class Program
    {
        public static bool isProductValid { get; set; }
        public static List<SummaryDataValidator> Validators { get; set; } = new List<SummaryDataValidator>();
        public static List<DetailedDataValidator> TotalDataValidators { get; set; } = new List<DetailedDataValidator>();
        public static List<SummaryDataValidator> ErrorSummaryValidators { get; set; } = new List<SummaryDataValidator>();
        public static List<DetailedDataValidator> ErrorDetailValidators { get; set; } = new List<DetailedDataValidator>();
        public static List<DetailedDataValidator> ConsolidatedErrors { get; set; } = new List<DetailedDataValidator>();
        public static List<Report> Reports { get; set; }= new List<Report>();
        public static Dictionary<string, JObject> jsonContents = new Dictionary<string, JObject>();

        public static Dictionary<string, string> matchedIds = new Dictionary<string, string>();

        private const string inputDirectory = @"Input";
        private const string outputDirectory = @"Output";

        static void Main(string[] args)
        {
            try
            {
                JObject mergedObject = new JObject();

                // Get all JSON files from the directory
                string[] jsonFiles = Directory.GetFiles(inputDirectory, "*.json");

                // Iterate through each JSON file
                foreach(string jsonFile in jsonFiles)
                {
                    // Read JSON file
                    string jsonContent = File.ReadAllText(jsonFile);

                    // Get duplicate product Ids in ItemsToBeModified
                    DataExt.GetIdsFromItemsToBeModified(jsonFile, jsonContent, ConsolidatedErrors);

                    // Parse JSON content
                    JObject jsonObject = JObject.Parse(jsonContent);

                    // Merge current JSON object into mergedObject
                    mergedObject.Merge(jsonObject);

                    // Store JSON content along with the file name
                    string fileName = Path.GetFileName(jsonFile);
                    jsonContents.Add(fileName, jsonObject);
                }

                // Merge input file
                if (mergedObject.Count >= 1)
                {
                    Ext.CreateOutputFolder(outputDirectory);
                    var mergedJSONOutput = $"{outputDirectory}/MergedInputJSON/Merged_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.json";
                    Console.WriteLine($"\nWriting merged JSON files to: {mergedJSONOutput}");
                    File.WriteAllText(mergedJSONOutput, mergedObject.ToString());
                }

                // Generate the report for extracted data
                GenerateDataReport();

                // Validate duplicate itemsForDeletion product ids in itemsToBeModified
                DataExt.ValidateDuplicateProductIds(jsonContents, ConsolidatedErrors);

                foreach (var jsonObject in jsonContents)
                {
                    // Validate itemsToBeModified
                    ValidateItemsToBeModified(jsonObject.Key, jsonObject.Value);
                }

                var consolidatedErrorOutput = $"{outputDirectory}/ConsolidatedErrors/ConsolidatedErrorDetails_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";

                // Merging the error result
                if(ConsolidatedErrors.Count >= 1)
                {
                    Console.WriteLine($"\nWriting consolidated errors output to: {consolidatedErrorOutput}");
                    FileUtils.WriteCsvFile(consolidatedErrorOutput, ConsolidatedErrors, new DetailedDataValidatorMap());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void GenerateDataReport()
        {
            foreach(var kvp in jsonContents)
            {
                JArray itemsForDeletion = (JArray)kvp.Value["itemsForDeletion"];
                JObject itemsToBeModified = (JObject)kvp.Value["itemsToBeModified"];

                foreach (var itemsDeletion in itemsForDeletion)
                {
                    var report = new Report()
                    {
                        FileName = kvp.Key.ToString(),
                        ProductId = itemsDeletion.ToString(),
                        gender = string.Empty,
                        ageGroup = string.Empty,
                        productType = string.Empty,
                        imageUrlIndexes = string.Empty,
                        Action = "itemsForDeletion"
                    };

                    Reports.Add(report);
                }

                foreach(var itemsModified in itemsToBeModified)
                {
                    string imageUrlIndexes = string.Empty;
                    if(itemsModified.Value["imageUrlIndexes"] != null)
                    {
                        imageUrlIndexes = string.Join(", ", ((JArray)itemsModified.Value["imageUrlIndexes"]).ToObject<int[]>());
                    }

                    var report = new Report()
                    {
                        FileName = kvp.Key.ToString(),
                        ProductId = itemsModified.Key.ToString(),
                        gender = itemsModified.Value["gender"] != null ? itemsModified.Value["gender"].ToString() : string.Empty,
                        ageGroup = itemsModified.Value["ageGroup"] != null ? itemsModified.Value["ageGroup"].ToString() : string.Empty,
                        productType = itemsModified.Value["productType"] != null ? itemsModified.Value["productType"].ToString() : string.Empty,
                        imageUrlIndexes = imageUrlIndexes != string.Empty ? "[" + imageUrlIndexes + "]" : string.Empty,
                        Action = "itemsToBeModified"
                    };

                    Reports.Add(report);
                }
            }

            Ext.CreateOutputFolder(outputDirectory);
            var reportDataOutput = $"{outputDirectory}/Report/Report_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";

            // Writing the consolidated data report output csv file
            if (Reports.Count >= 1)
            {
                Console.WriteLine($"\nWriting consolidated data report output to: {reportDataOutput}");
                FileUtils.WriteCsvFile(reportDataOutput, Reports, new ReportMap());
            }
        }

        private static void ValidateItemsToBeModified(string fileName, JObject jsonObject)
        {
            JObject itemsToBeModified = (JObject)jsonObject["itemsToBeModified"];
            if (itemsToBeModified != null)
            {
                Console.WriteLine("\nItems to Be Modified:");

                foreach (var item in itemsToBeModified)
                {
                    isProductValid = true;
                    string key = item.Key;
                    JObject properties = (JObject)item.Value;

                    List<string> validValues = TypeValidator.GetValidKeys();

                    foreach (var prop in properties)
                    {
                        if (!validValues.Contains(prop.Key))
                        {
                            // First output validation
                            var validator = new SummaryDataValidator()
                            {
                                FileName = fileName,
                                ProductName = key,
                                Validation = "Error"
                            };

                            // Second output validation
                            var totaldatavalidator = new DetailedDataValidator()
                            {
                                FileName = fileName,
                                ProductName = key,
                                Key = prop.Key,
                                Value = $"Invalid property '{prop.Key}' found for product {key}",
                                Validation = "Error"
                            };
                            
                            // Data validations
                            Validators.Add(validator);
                            TotalDataValidators.Add(totaldatavalidator);

                            // Error validations
                            ErrorSummaryValidators.Add(validator);
                            ErrorDetailValidators.Add(totaldatavalidator);

                            isProductValid = false;
                            Console.WriteLine($"Invalid property '{prop.Key}' found for item '{key}' in file.");
                        }
                    }

                    // Validate imageUrlIndexes
                    if (properties.ContainsKey("imageUrlIndexes"))
                    {
                        string imageUrlIndexes = properties["imageUrlIndexes"].ToString();
                        if (imageUrlIndexes != null)
                        {
                            // Parse the string into an array of integers
                            int[] array = Ext.ParseStringToArray(imageUrlIndexes);

                            // Check for duplicates
                            bool hasDuplicates = Ext.HasDuplicates(array);

                            if (hasDuplicates)
                            {
                                // First output validation
                                var validator = new SummaryDataValidator()
                                {
                                    FileName = fileName,
                                    ProductName = key,
                                    Validation = "Error"
                                };

                                // Second output validation
                                var totaldatavalidator = new DetailedDataValidator()
                                {
                                    FileName = fileName,
                                    ProductName = key,
                                    Key = "imageUrlIndexes",
                                    Value = $"Duplicate value found in 'imageUrlIndexes' for product '{key}'",
                                    Validation = "Error"
                                };

                                // Error validations
                                ErrorSummaryValidators.Add(validator);
                                ErrorDetailValidators.Add(totaldatavalidator);

                                // Data validations
                                isProductValid = false;
                                Validators.Add(validator);
                                TotalDataValidators.Add(totaldatavalidator);
                            }
                        }
                    }

                    // Validate ageGroup
                    if (properties.ContainsKey("ageGroup"))
                    {
                        string ageGroup = properties["ageGroup"].ToString();
                        ValidatePropertyValue("ageGroup", ageGroup, TypeValidator.GetValidAgeGroup(), key, fileName);
                    }

                    // Validate gender
                    if (properties.ContainsKey("gender"))
                    {
                        string gender = properties["gender"].ToString();
                        ValidatePropertyValue("gender", gender, TypeValidator.GetValidGender(), key, fileName);
                    }

                    // Validate productType
                    if (properties.ContainsKey("productType"))
                    {
                        // Access the "productType" property from the JObject
                        JToken productTypeToken = properties["productType"];

                        // Check if the value is a single string
                        if (productTypeToken.Type == JTokenType.String)
                        {
                            string productType = (string)productTypeToken;
                            ValidatePropertyValue("productType", productType.ToString(), TypeValidator.GetValidProductTypes(), key, fileName);
                        }
                        
                        // Check if the value is an array
                        else if (productTypeToken.Type == JTokenType.Array)
                        {
                            JArray productTypes = (JArray)productTypeToken;
                            foreach (var productType in productTypes)
                            {
                                ValidatePropertyValue("productType", productType.ToString(), TypeValidator.GetValidProductTypes(), key, fileName);
                            }
                        }
                    }

                    if (isProductValid)
                    {
                        // Summary data validation
                        var validator = new SummaryDataValidator()
                        {
                            FileName = fileName,
                            ProductName = key,
                            Validation = "Valid"
                        };

                        // Detailed data validation
                        var totaldatavalidator = new DetailedDataValidator()
                        {
                            FileName = fileName,
                            ProductName = key,
                            Key = "Valid keys",
                            Value = $"Valid values",
                            Validation = "Valid"
                        };

                        Validators.Add(validator);
                        TotalDataValidators.Add(totaldatavalidator);
                        isProductValid = true;
                    }
                }

                // Extract the fileName without extension
                fileName = Path.GetFileNameWithoutExtension(fileName);

                Ext.CreateOutputFolder(outputDirectory);
                var detailsOutput = $"{outputDirectory}/Details/{fileName}_Details_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";
                var summaryOutput = $"{outputDirectory}/Summary/{fileName}_Summary_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";
                var errorDetailsOutput = $"{outputDirectory}/ErrorDetails/{fileName}_ErrorDetails_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";
                var errorSummaryOutput = $"{outputDirectory}/ErrorSummary/{fileName}_ErrorSummary_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";

                // Writing the summary output csv file
                if(Validators.Count >= 1)
                {
                    Console.WriteLine($"\nWriting summary output to: {summaryOutput}");
                    FileUtils.WriteCsvFile(summaryOutput, Validators, new SummaryDataValidatorMap());
                }

                // Writing the details output csv file
                if(TotalDataValidators.Count >= 1)
                {
                    Console.WriteLine($"\nWriting details output to: {detailsOutput}");
                    FileUtils.WriteCsvFile(detailsOutput, TotalDataValidators, new DetailedDataValidatorMap());
                }

                // Writing the error details output csv file
                if(ErrorDetailValidators.Count >= 1)
                {
                    Console.WriteLine($"\nWriting error details output to: {errorDetailsOutput}");
                    FileUtils.WriteCsvFile(errorDetailsOutput, ErrorDetailValidators, new DetailedDataValidatorMap());
                }

                // Writing the error summary output csv file
                if(ErrorSummaryValidators.Count >= 1)
                {
                    Console.WriteLine($"\nWriting error summary output to: {errorSummaryOutput}");
                    FileUtils.WriteCsvFile(errorSummaryOutput, ErrorSummaryValidators, new SummaryDataValidatorMap());
                }

                // Consolidated errors
                ConsolidatedErrors.AddRange(ErrorDetailValidators);

                // Clearing the updated records
                Validators.Clear();
                TotalDataValidators.Clear();
                ErrorDetailValidators.Clear();
                ErrorSummaryValidators.Clear();
            }
            else
            {
                Console.WriteLine("No items to be modified found.");
            }
        }

        private static void ValidatePropertyValue(string propertyName, string propertyValue, List<string> validValues, string key, string fileName)
        {
            if (!validValues.Contains(propertyValue))
            {
                // Summary data output validation
                var validator = new SummaryDataValidator()
                {
                    FileName = fileName,
                    ProductName = key,
                    Validation = "Error"
                };

                // Detailed data output validation
                var totaldatavalidator = new DetailedDataValidator()
                {
                    FileName = fileName,
                    ProductName = key,
                    Key = propertyName,
                    Value = $"Invalid {propertyName} value '{propertyValue}' found for the product '{key}'",
                    Validation = "Error"
                };

                // Error validations
                ErrorSummaryValidators.Add(validator);
                ErrorDetailValidators.Add(totaldatavalidator);

                isProductValid = false;
                Validators.Add(validator);
                TotalDataValidators.Add(totaldatavalidator);
            }
        }
    }
}
