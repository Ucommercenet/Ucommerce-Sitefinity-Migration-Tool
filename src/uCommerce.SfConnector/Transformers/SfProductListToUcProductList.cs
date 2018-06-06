using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Script.Serialization;
using MigrationCommon.Data;
using MigrationCommon.Extensions;
using MigrationCommon.Models;
using NHibernate;
using NHibernate.Linq;
using timw255.Sitefinity.RestClient.Model;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;
using Product = UCommerce.EntitiesV2.Product;

namespace uCommerce.SfConnector.Transformers
{
    public class SfProductListToUcProductList : ITransformer<IEnumerable<ProductViewModel>, IEnumerable<Product>>
    {
        public string ConnectionString { private get; set; }
        public string SitefinitySiteName { private get; set; }
        public log4net.ILog Log { private get; set; }

        private ISession _session;
        private PriceGroup _defaultPriceGroup;

        public IEnumerable<Product> Execute(IEnumerable<ProductViewModel> sitefinityProducts)
        {
            _session = SessionFactory.Create(ConnectionString);

            var connection = SqlSessionFactory.Create(ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString);

            var catalog = _session.Query<ProductCatalog>().First(x => x.Name == SitefinitySiteName);
            _defaultPriceGroup = catalog.PriceGroup;

            var products = new List<Product>();

            try
            {
                foreach (var sfProduct in sitefinityProducts)
                {
                    var product = _session.Query<Product>()
                        .SingleOrDefault(a => a.Sku == sfProduct.Item.Sku && (a.VariantSku == null || a.VariantSku == string.Empty));

                    if (product != null)
                    {
                        Log.Warn($"product {product.Name.ToShortName(25)} with sku {sfProduct.Item.Sku} already exists in Ucommerce");
                        continue;
                    }

                    product = new Product();

                    AddProduct(product, sfProduct);
                    AddProductCategoryAssociations(product, sfProduct);
                    products.Add(product);
                }
            }
            finally
            {
                connection.Close();
                connection.Dispose();
                _session.Close();
            }

            return products;
        }

        private void AddProduct(Product product, ProductViewModel sfProduct)
        {
            // Product
            AddProductValueTypes(product, sfProduct);

            // Product Definiton ( Multilingual and Definitions )
            AddProductDescriptions(product, sfProduct);

            // Prices
            AddProductPrices(product, sfProduct.Item.Price);

            // Custom Properties
            AddProductProperties(product, sfProduct);

            // Inventory
            AddProductInventory(product, sfProduct.Item.InventoryJson);

            // Product Variants
            AddProductVariants(product, sfProduct);
        }

        private void AddProductInventory(Product product, string inventoryJson)
        {
            var productDefinitionField = _session.Query<ProductDefinitionField>().FirstOrDefault(x => x.Name == "InventoryOnHand"
                                             && x.ProductDefinition.ProductDefinitionId == product.ProductDefinition.Id);
            var inventoryModel = new JavaScriptSerializer().Deserialize<InventoryModel>(inventoryJson);

            var productProperty = new ProductProperty()
            {
                ProductDefinitionField = productDefinitionField,
                Value = inventoryModel.Inventory.ToString(),
                Product = product
            };
            product.AddProductProperty(productProperty);
        }

        private void AddProductValueTypes(Product product, ProductViewModel sfProduct)
        {
            product.Sku = sfProduct.Item.Sku;
            product.VariantSku = string.Empty;
            product.Name = sfProduct.Item.Title;
            product.DisplayOnSite = true;
            product.ThumbnailImageMediaId = string.Empty;
            product.PrimaryImageMediaId = string.Empty;
            if (sfProduct.Item.Weight != null) product.Weight = (decimal) sfProduct.Item.Weight;
            product.AllowOrdering = true;
            product.Rating = null;
            product.ProductDefinition = GetProductDefinition(sfProduct.ProductTypeTitle);
        }

        private void AddProductDescriptions(Product product, ProductViewModel sfProduct)
        {
            var displayName = sfProduct.Item.Title;
            var shortDescription = sfProduct.Item.Description;
            var longDescription = sfProduct.Item.Description;
       
            var desc = new ProductDescription
            {
                CultureCode = sfProduct.CultureCode,
                DisplayName = displayName,
                ShortDescription = shortDescription,
                LongDescription = longDescription
            };

            foreach (var translation in sfProduct.CultureTranslations)
            {
                var productDescription = new ProductDescription()
                {
                    CultureCode = translation.Key,
                    DisplayName = translation.Value.Item.Title,
                    ShortDescription = translation.Value.Item.Description,
                    LongDescription = translation.Value.Item.Description,
                    Product = product
                };

                product.AddProductDescription(productDescription);
            }

            product.AddProductDescription(desc);
        }

        private void AddProductDescriptionsForVariants(Product product, ProductVariation sfProduct)
        {
            var productDefinitionId = product.ProductDefinition.Id;
            var definition = _session.Query<ProductDefinition>().FirstOrDefault(x => x.ProductDefinitionId == productDefinitionId);

            if (definition == null) return;

            // Create product descriptions for each variant culture
            foreach (var translation in sfProduct.CultureTranslations)
            {
                var attributeNames = translation.Value.VariantNames.Attribute.Split(',');
                var attributeValues = translation.Value.VariantNames.AttributeValue.Split(',');
                var productDefinitionFieldNames = sfProduct.VariantNames.Attribute.Split(',');

                // There are no descriptions in Sitefinity at the variant level so for uCommerce we use
                // the concatenated variant values for display name + long & short descriptions
                var displayName = translation.Value.VariantNames.AttributeValue;
                var shortDescription = translation.Value.VariantNames.AttributeValue;
                var longDescription = translation.Value.VariantNames.AttributeValue;

                var productDescription = new ProductDescription
                {
                    CultureCode = translation.Key,
                    DisplayName = displayName,
                    ShortDescription = shortDescription,
                    LongDescription = longDescription
                };

                // Create product description properties for each variant value culture
                for (var i = 0; i < attributeNames.Length; i++)
                {
                    var fieldName = productDefinitionFieldNames[i];
                    var field = definition.ProductDefinitionFields.FirstOrDefault(x => x.Name == fieldName.Trim());

                    var productDescriptionProperty = new ProductDescriptionProperty
                    {
                        ProductDefinitionField = field,
                        ProductDescription = productDescription,
                        Value = attributeValues[i],
                        CultureCode = translation.Key
                    };

                    productDescription.AddProductDescriptionProperty(productDescriptionProperty);
                }

                product.AddProductDescription(productDescription);
            }
        }

        private ProductDefinition GetProductDefinition(string definitionName)
        {
            return _session.Query<ProductDefinition>().FirstOrDefault(x => x.Name == definitionName);
        }

        private void AddProductPrices(Product product, decimal price)
        {
            product.AddPriceGroupPrice(new PriceGroupPrice
            {
                PriceGroup = _defaultPriceGroup,
                Price = price
            });
        }

        private void AddProductCategoryAssociations(Product product,ProductViewModel sfProduct)
        {
            foreach (var categoryAssociation in sfProduct.CategoryAssociations)
            {
                var associatedCategory = GetCategory(categoryAssociation.ToString());

                if (associatedCategory == null)
                {
                    Log.Error($"Could not add product sku {product.Sku} to category.  Category not found.");
                    continue;
                }

                product.AddCategory(associatedCategory, 0);
            }
        }

        private void AddProductProperties(Product product, ProductViewModel sfProduct)
        {
            if (sfProduct.Item.CustomFields.Count == 0) return;

            // All properties are currently imported as ShortText properties
            var dataType = _session.Query<DataType>().FirstOrDefault(x => x.TypeName == "ShortText");
            foreach (var customProperty in sfProduct.Item.CustomFields)
            {
                if (customProperty.Key == "Department") continue;

                var property =
                    product.ProductDefinition.ProductDefinitionFields.FirstOrDefault(x => x.Name == customProperty.Key);

                if (property == null)
                {
                    property = new ProductDefinitionField
                    {
                        DisplayOnSite = true,
                        Deleted = false,
                        RenderInEditor = true,
                        IsVariantProperty = false,
                        SortOrder = 0,
                        Name = customProperty.Key,
                        DataType = dataType,
                        Multilingual = true

                    };
                    product.ProductDefinition.AddProductDefinitionField(property);
                }

                var propertyValue = GetPropertyValue(customProperty);
                var currentProductProperty = new ProductProperty
                {
                    Value = propertyValue,
                    ProductDefinitionField = property,
                    Product = product
                };

                product.AddProductProperty(currentProductProperty);
            }
        }

        private static string GetPropertyValue(KeyValuePair<string, object> customProperty)
        {
            var propertyValue = customProperty.Value.ToString();
            
            // Empty Object
            if (propertyValue == "[]" || propertyValue == "{}")
                return string.Empty;

            // Non-JSON value
            if (!propertyValue.StartsWith("{") && !propertyValue.StartsWith("["))
                return propertyValue;

            var propertyValueObj = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(propertyValue);
            if (propertyValueObj?["Value"] != null)
            {
                return propertyValueObj["Value"];
            }

            return string.Empty;
        }

        private void AddProductVariants(Product product, ProductViewModel sfProduct)
        {
            if (sfProduct.VariationCount == 0) return;

            foreach (var sfVariant in sfProduct.ProductVariations)
            {
                // Create the variant product
                var variantProduct = new Product
                { 
                    Sku = product.Sku,  
                    Name = product.Name, //sfVariant.DeltaPriceDisplay,  
                    VariantSku = sfVariant.Sku,
                    ProductDefinition = product.ProductDefinition,
                    DisplayOnSite    = true,
                    ParentProduct = product
                };

                AddVariantProperties(product, sfVariant, variantProduct);

                // To determine variant price, add variant additional price to parent price
                var parentPrice = product.PriceGroupPrices.First(p => p.PriceGroup.Name == _defaultPriceGroup.Name).Price ?? 0;
                var variantPrice = parentPrice + sfVariant.AdditionalPrice;
                AddProductPrices(variantProduct, variantPrice);

                AddProductDescriptionsForVariants(variantProduct, sfVariant);
                AddProductInventory(variantProduct, sfVariant.InventoryJson);
                product.AddVariant(variantProduct);
            }
        }

        private void AddVariantProperties(Product product, ProductVariation sfVariant, Product variantProduct)
        {
            // Variant names and values live in comma separated lists in the Sitefinity models
            var variantFieldNames = sfVariant.VariantNames.Attribute.Split(',');
            var variantValues = sfVariant.VariantNames.AttributeValue.Split(',');

            // Create the unique property values that make up the variant
            for (var i = 0; i < variantFieldNames.Length; i++)
            {
                var variantFieldName = variantFieldNames[i].Trim();
                var variantValue = variantValues[i].Trim();

                var productDefinitionField = _session.Query<ProductDefinitionField>().FirstOrDefault(x => x.Name == variantFieldName
                    && x.ProductDefinition.ProductDefinitionId == product.ProductDefinition.Id);

                if (productDefinitionField == null)
                {
                    Log.Error(
                        $"Could not add product variant for sku '{product.Sku}'.  Product definition field with name '{variantFieldName}' not found.");
                    continue;
                }

                var productProperty = new ProductProperty()
                {
                    ProductDefinitionField = productDefinitionField,
                    Value = variantValue,
                    Product = variantProduct
                };
                
                variantProduct.AddProductProperty(productProperty);
            }
        }

        private Category GetCategory(string sitefinityId)
        {
            return _session.Query<Category>().SingleOrDefault(
                x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == sitefinityId) == 1);
        }
    }
}
