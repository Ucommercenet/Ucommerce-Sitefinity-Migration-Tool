using System.Collections.Generic;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Receivers
{
    public class CatalogsFromSitefinity : Configurable, IReceiver<IEnumerable<SitefinityCatalog>>
    {
        public string DefaultCatalogName { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fetch catalog related data from Sitefinity
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SitefinityCatalog> Receive()
        {
            // Stub   TODO: currently we are defaulting to a single catalog

            return new List<SitefinityCatalog>()
            {
                new SitefinityCatalog()
                {
                    CatalogName = DefaultCatalogName,
                }
            };
        }
    }
}
