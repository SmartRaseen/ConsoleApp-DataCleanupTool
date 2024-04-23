using System.Collections.Generic;

namespace DataCleanupTool
{
    internal class TypeValidator
    {
        public static List<string> GetValidProductTypes()
        {
            // Define and return the list of valid product types
            return new List<string> { "shirt", "swimsuit", "shoes", "gloves", "boots", "jackets/coats", "glasses", "dress", "sandals", "bodysuit", "sweaters", "scarf", "pants", "ties", "leggings", "heels", "hats", "sneakers", "skirt", "gown", "pyjamas", "socks", "shorts", "male underwear", "female underwear", "beanie", "jumpsuit", "suits" };
        }

        public static List<string> GetValidGender()
        {
            // Define and return the list of valid gender
            return new List<string> { "women", "men", "unisex", "girls", "boys" };
        }

        public static List<string> GetValidAgeGroup()
        {
            // Define and return the list of valid gender
            return new List<string> { "baby", "toddler", "children", "teens", "adult" };
        }

        public static List<string> GetValidKeys()
        {
            // Define and return the list of valid key
            return new List<string> { "gender", "ageGroup", "productType", "imageUrlIndexes" };
        }
    }
}
