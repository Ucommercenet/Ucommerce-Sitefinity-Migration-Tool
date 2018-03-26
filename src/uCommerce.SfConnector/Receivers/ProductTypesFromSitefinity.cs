using System;
using System.Collections.Generic;
using System.Linq;
using MigrationCommon.Exceptions;
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
            var productTypes = new List<ProductTypeViewModel>();

            try
            {
                using (var sf = new SitefinityRestClient(SitefinityUsername, SitefinityPassword, SitefinityBaseUrl))
                {
                    Log.Info("fetching product types and properties from Sitefinity");
                    var productTypeWrapper = new ProductTypeServiceWrapper(sf);
                    productTypes = productTypeWrapper.GetProductTypes("", "", 0, int.MaxValue, "").Items.ToList();
                    Log.Info($"{productTypes.Count()} product types returned from Sitefinity");

                    GetProductProperties(productTypes, sf);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to fetch product type data from Sitefinity: \n{ex}");
                throw new MigrationException("A fatal exception occurred trying to fetch product type data from Sitefinity", ex);
            }

            return productTypes;
        }

        private static void GetProductProperties(IEnumerable<ProductTypeViewModel> productTypes, SitefinityRestClient sf)
        {
            var attribs = new ProductAttributeServiceWrapper(sf);
            var prodAttribs = attribs.GetProductAttributes("", "", 0, int.MaxValue, "");

            foreach (var productType in productTypes)
            {
                var attributes = prodAttribs.Items.Where(x => x.AppliedToJson.Contains(productType.Id.ToString())).ToList();
                if (attributes.Count > 0) productType.ProductAttributes = attributes;
            }
        }
    }
}