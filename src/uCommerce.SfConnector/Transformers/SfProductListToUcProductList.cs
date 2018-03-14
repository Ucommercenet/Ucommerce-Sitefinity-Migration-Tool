using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using uCommerce.uConnector.Model;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfProductListToUcProductList : ITransformer<IEnumerable<SitefinityProduct>, IEnumerable<Product>>
    {
        public string DefaultPriceGroupName { get; set; }
        public string DateTimeFormat { get; set; }
        public string DecimalFormat { get; set; }
        public string DoubleFormat { get; set; }

        public string CategoryPartSeperator { get; set; }

        private readonly CultureInfo _CultureInfo = new CultureInfo("en-US");

        public SfProductListToUcProductList()
        {
            CategoryPartSeperator = UCommerceProduct.Category.CATEGORY_PART_SEPERATOR;

            DateTimeFormat = UCommerceProduct.DATETIME_FORMAT;
            DecimalFormat = UCommerceProduct.DECIMAL_FORMAT;
            DoubleFormat = UCommerceProduct.DOUBLE_FORMAT;
        }

        public IEnumerable<Product> Execute(IEnumerable<SitefinityProduct> @from)
        {
            var connection = SqlSessionFactory.Create(ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString);
            var tempProducts = new List<Product>();

            foreach (var sfProduct in @from)
            {
                var product = new Product();
                product.Sku = sfProduct.Sku;
                product.VariantSku = "";
                product.Name = sfProduct.Title_;
                product.DisplayOnSite = true;
                product.ThumbnailImageMediaId = "";
                product.PrimaryImageMediaId = "";
                if (sfProduct.Weight != null) product.Weight = (decimal) sfProduct.Weight;

                product.ProductDefinition = new ProductDefinition()
                {
                    Name = sfProduct.TypeName
                };

                product.AllowOrdering = true;
                product.Rating = null;

                AddProductCategoryAssociations(connection, sfProduct.Id, product);
                AddProductPrice((decimal)sfProduct.Price, product);

                //foreach (var cultureCode in descriptionCultureCodes)   // TODO Culture Codes
                //{
                var displayName = sfProduct.Title_;
                var shortDescription = sfProduct.Description_;
                var longDescription = sfProduct.Description_;

                var desc = new ProductDescription();
                desc.CultureCode = _CultureInfo.Name; // TODO
                desc.DisplayName = displayName;
                desc.ShortDescription = shortDescription;
                desc.LongDescription = longDescription;

                product.ProductDescriptions.Add(desc);
                
                tempProducts.Add(product);
            }

            var finalList = new List<Product>();
            tempProducts.Where(x => string.IsNullOrWhiteSpace(x.VariantSku)).ToList().ForEach(finalList.Add);
            foreach (var product in tempProducts.Where(a => !string.IsNullOrWhiteSpace(a.VariantSku)))
            {
                var parentProduct =
                    tempProducts.SingleOrDefault(x => x.Sku == product.Sku && string.IsNullOrWhiteSpace(x.VariantSku));
                if (parentProduct == null)
                    throw new Exception(string.Format("Could not find matching parent Sku '{0}' for VariantSku: '{1}'",
                        product.Sku, product.VariantSku));

                parentProduct.Variants.Add(product);
            }

            connection.Close();
            connection.Dispose();

            return finalList;
        }

        private void AddProductPrice(decimal price, Product product)
        {
            product.AddPriceGroupPrice(new PriceGroupPrice
            {
                PriceGroup = new PriceGroup()
                {
                    Name = DefaultPriceGroupName
                },
                Price = price
            });
        }

        private static void AddProductCategoryAssociations(System.Data.IDbConnection connection, Guid productId, Product product)
        {
            var categoryAssociations = connection.Query<Guid>(
                "select taxa.id from sf_ec_product prod " +
                "join sf_ec_product_department dept on prod.id = dept.id " +
                "join sf_taxa taxa on dept.val = taxa.id " +
                $"where prod.id = '{productId}'");

            foreach (var categoryAssociation in categoryAssociations)
            {
                product.AddCategory(new Category
                {
                    Name = categoryAssociation.ToString(),
                    SortOrder = 0
                }, 0);
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
    }
}
