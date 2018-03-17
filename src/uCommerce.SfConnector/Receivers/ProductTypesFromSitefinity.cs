using System.Collections.Generic;
using System.Linq;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.Model;
using timw255.Sitefinity.RestClient.ServiceWrappers.Ecommerce.Catalog;
using UConnector.Framework;

namespace uCommerce.SfConnector.Adapters.Receivers
{
    public class ProductTypesFromSitefinity : Configurable, IReceiver<IEnumerable<ProductTypeViewModel>>
    {
        public string SitefinityBaseUrl { private get; set; }
        public string SitefinityUsername { private get; set; }
        public string SitefinityPassword { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fetch product definitions from Sitefinity
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProductTypeViewModel> Receive()
        {
            using (var sf = new SitefinityRestClient(SitefinityUsername, SitefinityPassword, SitefinityBaseUrl))
            {
                Log.Info("fetching product types from Sitefinity");
                var productTypeWrapper = new ProductTypeServiceWrapper(sf);
                var productTypes = productTypeWrapper.GetProductTypes("", "", 0, int.MaxValue, "").Items.ToList();
                Log.Info($"{productTypes.Count()} product types returned from Sitefinity");

                return productTypes;
            }
        }
    }
}
