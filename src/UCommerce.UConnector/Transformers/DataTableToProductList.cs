using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using UCommerce.EntitiesV2;
using uCommerce.uConnector.Model;
using UConnector.Extensions;
using UConnector.Framework;

namespace uCommerce.uConnector.Transformers
{
	public class DataTableToProductList : ITransformer<DataTable, IEnumerable<Product>>
    {
        public string DateTimeFormat { get; set; }
        public string DecimalFormat { get; set; }
        public string DoubleFormat { get; set; }

        public string CategoryPartSeperator { get; set; }

        private readonly CultureInfo _CultureInfo = new CultureInfo("en-US");

        public DataTableToProductList()
        {
            CategoryPartSeperator = UCommerceProduct.Category.CATEGORY_PART_SEPERATOR;

            DateTimeFormat = UCommerceProduct.DATETIME_FORMAT;
            DecimalFormat = UCommerceProduct.DECIMAL_FORMAT;
            DoubleFormat = UCommerceProduct.DOUBLE_FORMAT;
        }

        private void AddProductProperty(Product product, string name, string value)
        {
            var field = new ProductDefinitionField();
            field.Name = name;
            field.Multilingual = false;

            var productProperty = new ProductProperty();
            productProperty.Value = value;
            productProperty.ProductDefinitionField = field;
            product.ProductProperties.Add(productProperty);
        }

        private void AddProductDescriptionProperty(Product product, string name, string value, string cultureCode)
        {
            var field = new ProductDefinitionField();
            field.Name = name;
            field.Multilingual = true;

            var productDescription = product.ProductDescriptions.SingleOrDefault(x => x.CultureCode == cultureCode);
            if (productDescription == null)
                throw new NullReferenceException(
                    "productDescription should not be null since its parsed before 'ProductDescriptionProperties'");

            var productProperty = new ProductDescriptionProperty
                                      {
                                          Value = value, 
                                          ProductDefinitionField = field
                                      };

            productDescription.ProductDescriptionProperties.Add(productProperty);
        }

        public IEnumerable<Product> Execute(DataTable @from)
        {
            var tempProducts = new List<Product>();

            var priceColumns =
                @from.Columns.Cast<DataColumn>().Where(
                    a => a.ColumnName.StartsWith(UCommerceProduct.Price.GetColumnName(""))).ToList();

            var descriptionCultureCodes = @from.Columns
                .Cast<DataColumn>()
                .Where(a => a.ColumnName.StartsWith(UCommerceProduct.Description.PREFIX)
                            && a.ColumnName.EndsWith(UCommerceProduct.Description.DISPLAY_NAME))
                .Select(a => a.ColumnName.Split(new[] {"_"}, 3, StringSplitOptions.None)[1]).ToList();

            var categoryColumns =
                @from.Columns.Cast<DataColumn>().Where(
                    x => x.ColumnName.StartsWith(UCommerceProduct.Category.GetPrefix())).ToList();

            var fieldColumns =
                @from.Columns.Cast<DataColumn>().Where(
                    a => a.ColumnName.StartsWith(UCommerceProduct.Definition.GetName(""))).ToList();

            foreach (DataRow row in @from.Rows)
            {
                var product = new Product();
                product.Sku = row[UCommerceProduct.Columns.SKU].ToString();
                product.VariantSku = row[UCommerceProduct.Columns.VARIANT_SKU].ToString();
                product.Name = row[UCommerceProduct.Columns.NAME].ToString();
                product.DisplayOnSite = row[UCommerceProduct.Columns.DISPLAY_ON_SITE].ToBool();
                product.ThumbnailImageMediaId = row[UCommerceProduct.Columns.THUMBNAIL_IMAGE_MEDIA_ID].ToNullableInt().ToString();
                product.PrimaryImageMediaId = row[UCommerceProduct.Columns.PRIMARY_IMAGE_MEDIA_ID].ToNullableInt().ToString();
                product.Weight = row[UCommerceProduct.Columns.WEIGHT].ToDecimal(_CultureInfo);

                product.ProductDefinition = new ProductDefinition()
                                                {
                                                    Name = row[UCommerceProduct.Definition.GetDefinitionName()].ToEmptyString(),
                                                };
                
                product.AllowOrdering = row[UCommerceProduct.Columns.ALLOW_ORDERING].ToBool();
                product.Rating = row[UCommerceProduct.Columns.RATING].ToNullableDouble(_CultureInfo);

                foreach (var column in priceColumns)
                {
                    var price = row[column.ColumnName].ToNullableDecimal(_CultureInfo);
                    if (!price.HasValue)
                        continue;

                    var priceGroupPrice = new PriceGroupPrice
                                              {
                                                  Price = price
                                              };
                    priceGroupPrice.PriceGroup = new PriceGroup();
                    priceGroupPrice.PriceGroup.Name = column.ColumnName.Split(new[] {"_"}, 2, StringSplitOptions.RemoveEmptyEntries).Last();

                    product.PriceGroupPrices.Add(priceGroupPrice);
                }

                foreach (var cultureCode in descriptionCultureCodes)
                {
                    var displayName = row[UCommerceProduct.Description.DisplayName(cultureCode)];
                    var shortDescription = row[UCommerceProduct.Description.Short(cultureCode)];
                    var longDescription = row[UCommerceProduct.Description.Long(cultureCode)];

                    var desc = new ProductDescription();
                    desc.CultureCode = cultureCode;
                    desc.DisplayName = displayName.ToString();
                    desc.ShortDescription = shortDescription.ToString();
                    desc.LongDescription = longDescription.ToString();

                    product.ProductDescriptions.Add(desc);
                }

                foreach (var column in fieldColumns)
                {
                    var value = row[column].ToEmptyString();
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    // Depends on how many elements in the array its either DescriptionProperty(which is allways multilingual) or ProductProperty
                    var strings = column.ColumnName.Split(new[] { "_" }, StringSplitOptions.None);

                    switch (strings.Length)
                    {
                        case 2:
                            AddProductProperty(product, strings[1], value);
                            break;
                        case 3:
                            AddProductDescriptionProperty(product, strings[1], value, strings[2]);
                            break;
                        default:
                            throw new NotImplementedException(string.Format("Wrong format of '{0}', there should be 2 or 3 elements.", value));
                    }
                }

                foreach (var categoryColumn in categoryColumns)
                {
                    var path = row[categoryColumn.ColumnName].ToString();
                    if(string.IsNullOrWhiteSpace(path))
                        continue;

                    var parts = path.Split(new[] {CategoryPartSeperator}, StringSplitOptions.RemoveEmptyEntries);

                    Category category;

                    switch (parts.Length)
                    {
                        case 1:
                            category = new Category()
                                           {
                                               Name = parts.Last(),
                                           };
                            break;
                        case 2:
                            category = new Category()
                                           {
                                               Name = parts.Last(),
                                               ProductCatalog = new ProductCatalog()
                                                     {
                                                         Name = parts.First(),
                                                     }
                                           };
                            break;

                        case 3:
                            category = new Category()
                                           {
                                               Name = parts.Last(),
                                               ProductCatalog = new ProductCatalog()
                                                     {
                                                         Name = parts[1],
                                                         ProductCatalogGroup = new ProductCatalogGroup()
                                                                                   {
                                                                                       Name = parts.First()
                                                                                   }
                                                     }
                                           };
                            break;

                        default:
                            throw new Exception(
                                string.Format("The parsed string: '{0}' contains {1} items, there should be between 1 and 3. In the format: '{{ProductCatalogGroup}}{2}{{ProductCatalog}}{2}{{Category}}'.",
                            path, parts.Length, CategoryPartSeperator));
                    }

                    var categoryProductRelation = new CategoryProductRelation()
                                                        {
                                                            Product = product,
                                                            Category = category,
                                                            SortOrder = 0,
                                                        };
                        
                    product.CategoryProductRelations.Add(categoryProductRelation);
                }

                tempProducts.Add(product);
            }

            var finalList = new List<Product>();
            tempProducts.Where(x => string.IsNullOrWhiteSpace(x.VariantSku)).ToList().ForEach(finalList.Add);
            foreach (var product in tempProducts.Where(a => !string.IsNullOrWhiteSpace(a.VariantSku)))
            {
                var parentProduct = tempProducts.SingleOrDefault(x => x.Sku == product.Sku && string.IsNullOrWhiteSpace(x.VariantSku));
                if (parentProduct == null)
                    throw new Exception(string.Format("Could not find matching parent Sku '{0}' for VariantSku: '{1}'", product.Sku, product.VariantSku));

                parentProduct.Variants.Add(product);
            }

            return finalList;
        }
    }
}