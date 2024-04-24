using System;
using System.IO;
using Newtonsoft.Json.Linq;
using DataCleanupTool.Model;
using DataCleanupTool.Utils;
using DataCleanupTool.Schema;
using System.Collections.Generic;
using Ext = DataCleanupTool.ValidatorExtension;
using DataExt = DataCleanupTool.DataValidatorExtension;

namespace DataCleanupTool
{
    internal class Program
    {
        public static bool isProductValid { get; set; }
        public static List<Validator> Validators { get; set; } = new List<Validator>();
        public static List<TotalDataValidator> TotalDataValidators { get; set; } = new List<TotalDataValidator>();
        public static List<Validator> ErrorSummaryValidators { get; set; } = new List<Validator>();
        public static List<TotalDataValidator> ErrorDetailValidators { get; set; } = new List<TotalDataValidator>();
        public static List<TotalDataValidator> ConsolidatedErrors { get; set; } = new List<TotalDataValidator>();
        public static Dictionary<string, JObject> jsonContents = new Dictionary<string, JObject>();

        public static Dictionary<string, string> matchedIds = new Dictionary<string, string>();

        private const string inputDirectory = @"Input";
        private const string outputDirectory = @"Output";

        static void Main(string[] args)
        {

            try
            {
                // Get all JSON files from the directory
                string[] jsonFiles = Directory.GetFiles(inputDirectory, "*.json");

                // Iterate through each JSON file
                foreach(string jsonFile in jsonFiles)
                {
                    // Read JSON file
                    string jsonContent = File.ReadAllText(jsonFile);

                    // Parse JSON content
                    JObject jsonObject = JObject.Parse(jsonContent);

                    // Store JSON content along with the file name
                    string fileName = Path.GetFileName(jsonFile);
                    jsonContents.Add(fileName, jsonObject);
                }

                // Validate duplicate product ids
                DataExt.ValidateDuplicateProductIds(jsonContents);

                foreach (var jsonObject in jsonContents)
                {
                    // Validate itemsForDeletion
                    ValidateItemsForDeletion(jsonObject.Key, jsonObject.Value);

                    // Validate itemsToBeModified
                    ValidateItemsToBeModified(jsonObject.Key, jsonObject.Value);
                }

                var consolidatedErrorOutput = $"{outputDirectory}/ConsolidatedErrors/ConsolidatedErrorDetails_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.csv";

                // Merging the error result
                FileUtils.WriteCsvFile(consolidatedErrorOutput, ConsolidatedErrors, new TotalDataValidatorMap());

                //
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ValidateItemsForDeletion(string fileName, JObject jsonObject)
        {
            JArray itemsForDeletion = (JArray)jsonObject["itemsForDeletion"];
            if (itemsForDeletion != null)
            {
                Console.WriteLine("Items for Deletion:");

                // Checking itemsForDeletion productIds are presented in itemsToBeModified
                JObject itemsToBeModified = (JObject)jsonObject["itemsToBeModified"];

                foreach (string item in itemsForDeletion)
                {
                    if (itemsToBeModified.ContainsKey(item))
                    {
                        // First output validation
                        var validator = new Validator()
                        {
                            FileName = fileName,
                            ProductName = item,
                            Validation = "Error"
                        };

                        // Second output validation
                        var totaldatavalidator = new TotalDataValidator()
                        {
                            FileName = fileName,
                            ProductName = item,
                            Key = "itemsForDeletion",
                            Value = $"This {item} product have duplicates in itemsToBeModified",
                            Validation = "Error"
                        };

                        // Data validations
                        Validators.Add(validator);
                        TotalDataValidators.Add(totaldatavalidator);

                        // Error validations
                        ErrorSummaryValidators.Add(validator);
                        ErrorDetailValidators.Add(totaldatavalidator);
                    }
                }
            }
            else
            {
                Console.WriteLine("No items for deletion found.");
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
                            var validator = new Validator()
                            {
                                FileName = fileName,
                                ProductName = key,
                                Validation = "Error"
                            };

                            // Second output validation
                            var totaldatavalidator = new TotalDataValidator()
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
                                var validator = new Validator()
                                {
                                    FileName = fileName,
                                    ProductName = key,
                                    Validation = "Error"
                                };

                                // Second output validation
                                var totaldatavalidator = new TotalDataValidator()
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
                        // Data summary validation
                        var validator = new Validator()
                        {
                            FileName = fileName,
                            ProductName = key,
                            Validation = "Valid"
                        };

                        // Data detail validation
                        var totaldatavalidator = new TotalDataValidator()
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
                    FileUtils.WriteCsvFile(detailsOutput, Validators, new ValidatorMap());
                }

                // Writing the details output csv file
                if(TotalDataValidators.Count >= 1)
                {
                    Console.WriteLine($"\nWriting details output to: {detailsOutput}");
                    FileUtils.WriteCsvFile(summaryOutput, TotalDataValidators, new TotalDataValidatorMap());
                }

                // Writing the error details output csv file
                if(ErrorDetailValidators.Count >= 1)
                {
                    Console.WriteLine($"\nWriting error details output to: {errorDetailsOutput}");
                    FileUtils.WriteCsvFile(errorDetailsOutput, ErrorDetailValidators, new TotalDataValidatorMap());
                }

                // Writing the error summary output csv file
                if(ErrorSummaryValidators.Count >= 1)
                {
                    Console.WriteLine($"\nWriting error summary output to: {errorSummaryOutput}");
                    FileUtils.WriteCsvFile(errorSummaryOutput, ErrorSummaryValidators, new ValidatorMap());
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
                // First output validation
                var validator = new Validator()
                {
                    FileName = fileName,
                    ProductName = key,
                    Validation = "Error"
                };

                // Second output validation
                var totaldatavalidator = new TotalDataValidator()
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
