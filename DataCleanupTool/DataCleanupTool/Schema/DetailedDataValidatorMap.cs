using DataCleanupTool.Model;
using CsvHelper.Configuration;

namespace DataCleanupTool.Schema
{
    public class DetailedDataValidatorMap: ClassMap<DetailedDataValidator>
    {
        public DetailedDataValidatorMap() 
        {
            Map(m => m.FileName).Name("FileName");
            Map(m => m.ProductName).Name("ProductName");
            Map(m => m.Key).Name("Key");
            Map(m => m.Value).Name("Value");
            Map(m => m.Validation).Name("Validation");
        }
    }
}
