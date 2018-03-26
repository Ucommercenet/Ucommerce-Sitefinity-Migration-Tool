using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.Model;
using timw255.Sitefinity.RestClient.ServiceWrappers.Ecommerce.Catalog;
using uCommerce.SfConnector.Helpers;
using UConnector.Framework;

namespace uCommerce.SfConnector.Adapters.Receivers
{
    /// <summary>
    /// Receiver for product data
    /// </summary>
    public class ProductListFromSitefinity : Configurable, IReceiver<IEnumerable<ProductViewModel>>
    {
        public string SitefinityBaseUrl { private get; set; }
        public string SitefinityUsername { private get; set; }
        public string SitefinityPassword { private get; set; }
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        private ProductVariationServiceWrapper _productVariationServiceWrapper;

        /// <summary>
        /// Fetch product data from Sitefinity
        /// </summary>
        /// <returns>A list of sitefinity products</returns>
        public IEnumerable<ProductViewModel> Receive()
        {
            var products = new List<ProductViewModel>();
            try
            {
                using (var sf = new SitefinityRestClient(SitefinityUsername, SitefinityPassword, SitefinityBaseUrl))
                {
                    Log.Info("fetching products from Sitefinity");
                    var productWrapper = new ProductServiceWrapper(sf);
                    products = productWrapper.GetProducts("", "", 0, int.MaxValue, "", "", "").Items.ToList();
                    Log.Info($"{products.Count()} product returned from Sitefinity");

                    Log.Info("fetching product category associations");
                    AddCategoryAssociations(products);

                    Log.Info("fetching product attributes");
                    AddProductAttributeValues(products, sf);

                    Log.Info("fetching product variants");
                    AddProductVariants(products, sf);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to fetch product data from Sitefinity: \n{ex}");
            }

            return products;
        }

        private void AddProductVariants(List<ProductViewModel> products, SitefinityRestClient sf)
        {
            var serviceWrapper = GetProductVariationServiceWrapper(sf);

            foreach (var product in products.Where(x => x.VariationCount > 0))
            {
                var productVariants = serviceWrapper.GetProductVariationsOfParent(product.Id, "", "", 0, int.MaxValue, "");
                product.ProductVariations = productVariants.Items.ToList();
            }
        }

        private void AddProductAttributeValues(List<ProductViewModel> products, SitefinityRestClient sf)
        {                  
            var serviceWrapper = GetProductVariationServiceWrapper(sf);
            foreach (var product in products)
            {
                var attributeValues = serviceWrapper.GetAttributeValues(product.Id, "");
                var productAttributeValues = attributeValues.Items.ToList();
                product.ProductAttributeValues = productAttributeValues;
            }
        }

        private void AddCategoryAssociations(List<ProductViewModel> products)
        {
            // Strangely, product category associations do not seem to be available via the 
            // sf wcf web api.  So, until a better way is found, we need to grab them from the database.
            using (var connection = SqlSessionFactory.Create(ConnectionString))
            {
                foreach (var product in products)
                {
                    var categoryAssociations = connection.Query<Guid>(
                        "select taxa.id from sf_ec_product prod " +
                        "join sf_ec_product_department dept on prod.id = dept.id " +
                        "join sf_taxa taxa on dept.val = taxa.id " +
                        $"where prod.id = '{product.Item.Id}'");

                    product.CategoryAssociations = categoryAssociations;
                }
            }
        }

        private ProductVariationServiceWrapper GetProductVariationServiceWrapper(SitefinityRestClient sf)
        {
            if (_productVariationServiceWrapper != null) return _productVariationServiceWrapper;
            _productVariationServiceWrapper = new ProductVariationServiceWrapper(sf);

            return _productVariationServiceWrapper;
        }
    }
}
