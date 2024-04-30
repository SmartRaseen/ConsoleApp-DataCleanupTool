using CsvHelper.Configuration;
using DataCleanupTool.Model;

namespace DataCleanupTool.Schema
{
    public class ReportMap: ClassMap<Report>
    {
        public ReportMap() 
        {
            Map(m => m.FileName).Name("FileName");
            Map(m => m.ProductId).Name("ProductId");
            Map(m => m.gender).Name("gender");
            Map(m => m.ageGroup).Name("ageGroup");
            Map(m => m.productType).Name("productType");
            Map(m => m.imageUrlIndexes).Name("imageUrlIndexes");
            Map(m => m.Action).Name("Action");
        }
    }
}
