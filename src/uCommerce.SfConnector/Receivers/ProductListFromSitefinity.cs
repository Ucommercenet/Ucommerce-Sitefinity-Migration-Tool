using System;
using System.Collections.Generic;
using System.Data;
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

        /// <summary>
        /// Fetch product data from Sitefinity
        /// </summary>
        /// <returns>A list of sitefinity products</returns>
        public IEnumerable<ProductViewModel> Receive()
        {
            using (var sf = new SitefinityRestClient(SitefinityUsername, SitefinityPassword, SitefinityBaseUrl))
            {
                Log.Info("fetching products from Sitefinity");
                var productWrapper = new ProductServiceWrapper(sf);
                var products = productWrapper.GetProducts("", "", 0, int.MaxValue, "", "", "").Items.ToList();
                Log.Info($"{products.Count()} product returned from Sitefinity");

                Log.Info($"fetching product category associations");
                using (var connection = SqlSessionFactory.Create(ConnectionString))
                {
                    foreach (var product in products)
                    {
                        AddCategoryAssociations(connection, product);
                    }
                }

                return products;
            }
        }

        private void AddCategoryAssociations(IDbConnection connection, ProductViewModel product)
        {
            // Strangely, product category associations do not seem to be available via the 
            // sf wcf web api.  So, until a better way is found, we need to grab them from the database.
            var categoryAssociations = connection.Query<Guid>(
                "select taxa.id from sf_ec_product prod " +
                "join sf_ec_product_department dept on prod.id = dept.id " +
                "join sf_taxa taxa on dept.val = taxa.id " +
                $"where prod.id = '{product.Item.Id}'");

            product.CategoryAssociations = categoryAssociations;
        }
    }
}
