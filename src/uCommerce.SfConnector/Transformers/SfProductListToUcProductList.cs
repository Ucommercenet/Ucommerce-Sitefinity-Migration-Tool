using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Dapper;
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

        private readonly CultureInfo _CultureInfo = new CultureInfo("en-US");
        private ISession _session;

        public IEnumerable<Product> Execute(IEnumerable<ProductViewModel> sitefinityProducts)
        {
            _session = SessionFactory.Create(ConnectionString);
            var connection = SqlSessionFactory.Create(ConfigurationManager
                .ConnectionStrings["SitefinityConnectionString"].ConnectionString);
            var products = new List<Product>();

            try
            {
                foreach (var sfProduct in sitefinityProducts)
                {
                    var product = _session.Query<Product>()
                        .SingleOrDefault(a => a.Sku == sfProduct.Item.Sku && (a.VariantSku == null || a.VariantSku == string.Empty));
                    if (product != null) continue;

                    product = new Product();
                    AddProduct(product, sfProduct);

                    AddProductCategoryAssociations(product, sfProduct);
                    AddProductPrices(sfProduct.Item.Price, product);

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
            AddProductPrices(sfProduct.Item.Price, product);

            // ProductProperties
            // TODO

            // Variants
            // TODO
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

        private void AddProductPrices(decimal price, Product product)
        {
            // TODO account for potential multiple price groups
            var priceGroup = _session.Query<PriceGroup>().FirstOrDefault(a => a.Name == DefaultPriceGroupName);
            product.AddPriceGroupPrice(new PriceGroupPrice
            {
                PriceGroup = priceGroup,
                Price = price
            });
        }

        private void AddProductCategoryAssociations(Product product,ProductViewModel sfProduct)
        {
            foreach (var categoryAssociation in sfProduct.CategoryAssociations)
            {
                var associatedCategory = GetCategory(categoryAssociation.ToString());
                product.AddCategory(associatedCategory, 0);
            }
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

        private Category GetCategory(string sitefinityId)
        {
            return _session.Query<Category>().SingleOrDefault(
                x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == sitefinityId) == 1);
        }
    }
}
