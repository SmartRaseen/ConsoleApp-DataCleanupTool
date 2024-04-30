using DataCleanupTool.Model;
using CsvHelper.Configuration;

namespace DataCleanupTool.Schema
{
    public class SummaryDataValidatorMap: ClassMap<SummaryDataValidator>
    {
        public SummaryDataValidatorMap() 
        {
            Map(m => m.FileName).Name("FileName");
            Map(m => m.ProductName).Name("ProductName");
            Map(m => m.Validation).Name("Validation");
        }
    }
}
