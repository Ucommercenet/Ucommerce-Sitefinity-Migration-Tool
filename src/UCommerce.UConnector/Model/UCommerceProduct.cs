using UConnector.Extensions;

namespace uCommerce.uConnector.Model
{
    public static class UCommerceProduct
    {
        public const string PRODUCTS = "Products";

        public const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public const string DECIMAL_FORMAT = "0.0";
        public const string DOUBLE_FORMAT = "0.0";

        public static class Columns
        {
            public const string SKU = "Sku";
            public const string VARIANT_SKU = "VariantSku";
            public const string NAME = "Name";
            public const string DISPLAY_ON_SITE = "DisplayOnSite";
            public const string THUMBNAIL_IMAGE_MEDIA_ID = "ThumbnailImageMediaId";
            public const string PRIMARY_IMAGE_MEDIA_ID = "PrimaryImageMediaId";
            public const string WEIGHT = "Weight";
            public const string RATING = "Rating";
            
            public const string ALLOW_ORDERING = "AllowOrdering";
            
            //public const string COL_MODIFIED_BY = "ModifiedBy";
            //public const string COL_MODIFIED_ON = "ModifiedOn";
            //public const string COL_CREATED_ON = "CreatedOn";
            //public const string COL_CREATED_BY = "CreatedBy";
        }

        public static class Definition
        {
            public static string GetName(string fieldName)
            {
                return FIELD_FORMAT.FormatWith(fieldName);
            }

            public static string GetName(string fieldName, string cultureCode)
            {
                return FIELD_WITH_CULTURE_CODE_FORMAT.FormatWith(fieldName, cultureCode);
            }

            public static string GetDefinitionName()
            {
                return NAME;
            }

            /// <summary>
            /// Needs CultureCode and FieldName
            /// </summary>
            private const string FIELD_WITH_CULTURE_CODE_FORMAT = "Field_{0}_{1}";
            private const string FIELD_FORMAT = "Field_{0}";
            private const string NAME = "ProductDefinition";
        }

        public static class Category
        {
            public static string GetName(int i)
            {
                return NAME_FORMAT.FormatWith(i);
            }

            public static string GetPrefix()
            {
                return NAME_FORMAT.FormatWith("");
            }

            public const string NAME_FORMAT = "Category_{0}";
            public const string CATEGORY_PART_SEPERATOR = "/";
        }

        public static class Price
        {
            /// <summary>
            /// Need PriceGroupName
            /// </summary>
            private const string PRICE_FORMAT = "Price_{0}";

            public static string GetColumnName(string name)
            {
                return PRICE_FORMAT.FormatWith(name);
            }
        }

        public static class Description
        {
            public const string FORMAT = PREFIX + "{0}_{1}";
            public const string PREFIX = "Desc_";

            public const string DISPLAY_NAME = "DisplayName";
            public const string SHORT_DESC = "ShortDescription";
            public const string LONG_DESC = "LongDescription";

            public static string DisplayName(string cultureCode)
            {
                return FORMAT.FormatWith(cultureCode, DISPLAY_NAME);
            }

            public static string Long(string cultureCode)
            {
                return FORMAT.FormatWith(cultureCode, LONG_DESC);
            }

            public static string Short(string cultureCode)
            {
                return FORMAT.FormatWith(cultureCode, SHORT_DESC);
            }
        }
    }
}