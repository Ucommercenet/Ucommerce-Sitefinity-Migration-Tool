using System;
using System.Collections.Generic;
using System.Linq;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.Model;
using timw255.Sitefinity.RestClient.ServiceWrappers.Taxonomies;
using UConnector.Framework;

namespace uCommerce.SfConnector.Receivers
{
    public class TaxonomyFromSitefinity : Configurable, IReceiver<IEnumerable<WcfHierarchicalTaxon>>
    {
        public string SitefinityDepartmentTaxonomyId { private get; set; }
        public string SitefinityBaseUrl { private get; set; }
        public string SitefinityUsername { private get; set; }
        public string SitefinityPassword { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fetch departments from Sitefinity
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WcfHierarchicalTaxon> Receive()
        {
            var categories = new List<WcfHierarchicalTaxon>();
            try
            {
                using (var sf = new SitefinityRestClient(SitefinityUsername, SitefinityPassword, SitefinityBaseUrl))
                {
                    Log.Info("fetching product types from Sitefinity");
                    var categoriesWrapper = new HierarchicalTaxonServiceWrapper(sf);
                    categories = categoriesWrapper
                        .GetTaxa(new Guid(SitefinityDepartmentTaxonomyId), "", "", 0, 0, "", "", false, "").Items
                        .ToList();
                    Log.Info($"{categories.Count()} departments returned from Sitefinity");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to fetch department data from Sitefinity: \n{ex}");
            }

            return categories;
        }
    }
}
