using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using MigrationCommon.Extensions;
using NHibernate;
using NHibernate.Linq;
using timw255.Sitefinity.RestClient.Model;
using uCommerce.SfConnector.Helpers;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;
using Product = UCommerce.EntitiesV2.Product;

namespace uCommerce.SfConnector.Transformers
{
    public class SfProductListToUcProductList : ITransformer<IEnumerable<ProductViewModel>, IEnumerable<Product>>
    {
        public string DefaultPriceGroupName { get; set; }
        public string CategoryPartSeperator { get; set; }
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        private readonly CultureInfo _CultureInfo = new CultureInfo("en-US");
        private ISession _session;
        private PriceGroup _defaultPriceGroup;

        public IEnumerable<Product> Execute(IEnumerable<ProductViewModel> sitefinityProducts)
        {
            _session = SessionFactory.Create(ConnectionString);
            _defaultPriceGroup = _session.Query<PriceGroup>().First(a => a.Name == DefaultPriceGroupName);

            var connection = SqlSessionFactory.Create(ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString);
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

            // Product Variants
            AddProductVariants(product, sfProduct);
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
            // TODO: Account for culture specific descriptions
            var displayName = sfProduct.Item.Title;
            var shortDescription = sfProduct.Item.Description;
            var longDescription = sfProduct.Item.Description;

            var desc = new ProductDescription
            {
                CultureCode = _CultureInfo.Name,
                DisplayName = displayName,
                ShortDescription = shortDescription,
                LongDescription = longDescription
            };

            product.AddProductDescription(desc);
        }

        private ProductDefinition GetProductDefinition(string definitionName)
        {
            return _session.Query<ProductDefinition>().FirstOrDefault(x => x.Name == definitionName);
        }

        private void AddProductPrices(Product product, decimal price)
        {
            // TODO account for potential multiple price groups
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

        private void AddProductProperty(Product product, string name, string value)
        {

            var productDefinitionField = _session.Query<ProductDefinitionField>().FirstOrDefault(x => x.Name == name
                             && x.ProductDefinition.ProductDefinitionId == product.ProductDefinition.Id);

            if (productDefinitionField == null)
            {
                Log.Error($"Could not add product property value '{value}' for sku '{product.Sku}'.  Product definition field with name '{name}' not found.");
                return;
            }

            var currentProductProperty = product.GetProperties().Cast<ProductProperty>()
                .SingleOrDefault(x => x.ProductDefinitionField.Name == name);
            if (currentProductProperty != null)
            {
                currentProductProperty.Value = value;
                return;
            }

            currentProductProperty = new ProductProperty
            {
                Value = value,
                ProductDefinitionField = productDefinitionField,
                Product = product
            };
            product.AddProductProperty(currentProductProperty);
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

        private void AddProductVariants(Product product, ProductViewModel sfProduct)
        {
            if (sfProduct.VariationCount == 0) return;

            foreach (var sfVariant in sfProduct.ProductVariations)
            {  
                // Create the variant product
                var variantProduct = new Product
                { 
                    Sku = product.Sku,   
                    Name = sfVariant.DeltaPriceDisplay,  
                    VariantSku = sfVariant.Sku,
                    ProductDefinition = product.ProductDefinition,
                    DisplayOnSite    = true,
                    ParentProduct = product
                };

                AddVariantProperties(product, sfVariant, variantProduct);

                // To determine variant price, add variant additional price to parent price
                var parentPrice = product.PriceGroupPrices.First(p => p.PriceGroup.Name == DefaultPriceGroupName).Price ?? 0;
                var variantPrice = parentPrice + sfVariant.AdditionalPrice;
                AddProductPrices(variantProduct, variantPrice);

                product.AddVariant(variantProduct);
            }
        }

        private void AddVariantProperties(Product product, ProductVariation sfVariant, Product variantProduct)
        {
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
