using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DataCleanupTool
{
    public class ValidatorExtension
    {
        public static int[] ParseStringToArray(string data)
        {
            // Remove whitespace and square brackets, then split by comma
            string[] elements = data.Replace("[", "").Replace("]", "").Split(',');

            // Parse each element into an integer
            int[] array = elements.Select(e => int.Parse(e.Trim())).ToArray();

            return array;
        }

        public static bool HasDuplicates(int[] array)
        {
            // Use a HashSet to check for duplicates
            HashSet<int> set = new HashSet<int>();

            foreach (int element in array)
            {
                if (!set.Add(element))
                {
                    // If adding the element fails, it's a duplicate
                    return true;
                }
            }

            return false;
        }

        public static void CreateOutputFolder(string outputDirectory)
        {
            var resultDetailsFolder = $@"{outputDirectory}\Details";
            var resultSummaryFolder = $@"{outputDirectory}\Summary";
            var resultErrorDetailsFolder = $@"{outputDirectory}\ErrorDetails";
            var resultErrorSummaryFolder = $@"{outputDirectory}\ErrorSummary";
            var resultConsolidatedErrorFolder = $@"{outputDirectory}\ConsolidatedErrors";
            var resultDuplicateProductIdsFolder = $@"{outputDirectory}\DuplicateProductIds";

            if (!Directory.Exists(resultDetailsFolder))
            {
                Directory.CreateDirectory(resultDetailsFolder);
            }

            if (!Directory.Exists(resultSummaryFolder))
            {
                Directory.CreateDirectory(resultSummaryFolder);
            }

            if (!Directory.Exists(resultErrorDetailsFolder))
            {
                Directory.CreateDirectory(resultErrorDetailsFolder);
            }

            if (!Directory.Exists(resultErrorSummaryFolder))
            {
                Directory.CreateDirectory(resultErrorSummaryFolder);
            }

            if (!Directory.Exists(resultConsolidatedErrorFolder))
            {
                Directory.CreateDirectory(resultConsolidatedErrorFolder);
            }

            if (!Directory.Exists(resultDuplicateProductIdsFolder))
            {
                Directory.CreateDirectory(resultDuplicateProductIdsFolder);
            }
        }
    }
}
