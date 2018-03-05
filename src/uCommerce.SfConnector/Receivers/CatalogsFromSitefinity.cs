using System.Collections.Generic;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Receivers
{
    public class CatalogsFromSitefinity : Configurable, IReceiver<IEnumerable<SitefinityCatalog>>
    {
        public string ConnectionString { private get; set; }

        public IEnumerable<SitefinityCatalog> Receive()
        {
            // Stub TODO: however we extract catalogs...

            return new List<SitefinityCatalog>()
            {
                new SitefinityCatalog()
                {
                    CatalogName = "Albert New Catalog",
                }
            };
        }
    }
}
