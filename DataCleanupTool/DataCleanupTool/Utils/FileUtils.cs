using System.IO;
using System.Text;
using CsvHelper.Configuration;
using System.Collections.Generic;

namespace DataCleanupTool.Utils
{
    internal static class FileUtils
    {
        public static void WriteCsvFile<T>(string filePath, List<T> data, ClassMap<T> classMap)
        {
            using TextWriter writer = new StreamWriter(filePath, false, Encoding.UTF8);
            var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            if (csv is null)
            {
                return;
            }  
            
            csv.Context.RegisterClassMap(classMap);
            csv.WriteRecords(data);
        }
    }
}
