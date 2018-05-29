using System;
using System.Collections.Generic;
using System.Linq;
using MigrationCommon.Data;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.Model;
using timw255.Sitefinity.RestClient.ServiceWrappers.Ecommerce.Catalog;
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
        public int Skip { private get; set; }
        public int Take { private get; set; }
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
                    Log.Info($"fetching product batch {Skip}-{Skip+Take} from Sitefinity");
                    products = GetProductsForAllCultures(sf);

                    Log.Info($"{products.Count()} products returned from Sitefinity");

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
                throw;
            }

            return products;
        }

        private List<ProductViewModel> GetProductsForAllCultures(SitefinityRestClient sf)
        {
            var culturesToMigrate = DataHelper.GetCulturesToMigrate(sf).ToList();
            List<ProductViewModel> products;
            var productWrapper = new ProductServiceWrapper(sf);
            var defaultCulture = culturesToMigrate.First(x => x.IsDefault);
            products = productWrapper.GetProducts("", "", Skip, Take, "", "", "", defaultCulture.Culture).Items.ToList();
            
            AddCultureToProducts(products, defaultCulture);

            var secondaryCultures = culturesToMigrate.Where(x => x.IsDefault == false);
            foreach (var culture in secondaryCultures)
            {
                var cultureProducts = productWrapper
                    .GetProducts("", "", Skip, Take, "", "", "", culture.Culture).Items
                    .ToList();

                AddSecondaryCulturesToProducts(products, cultureProducts, culture.Culture);
            }

            return products;
        }

        private void AddCultureToProducts(List<ProductViewModel> products, CultureViewModel defaultCulture)
        {
            foreach (var product in products)
            {
                product.CultureCode = defaultCulture.Culture;
            }
        }

        private static void AddSecondaryCulturesToProducts(List<ProductViewModel> products, List<ProductViewModel> cultureProducts, string culture)
        {
            foreach (var product in products)
            {
                var translation = cultureProducts.First(x => x.Id == product.Id);
                product.CultureTranslations.Add(culture, translation);
            }
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
            foreach (var product in products)
            {
                product.CategoryAssociations = DataHelper.GetProductCategoryAssociations(product.Item.Id.ToString());
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
