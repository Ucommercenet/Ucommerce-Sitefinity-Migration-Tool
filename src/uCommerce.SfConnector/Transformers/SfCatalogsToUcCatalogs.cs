using System.Collections.Generic;
using uCommerce.SfConnector.Model;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfCatalogsToUcCatalogs : ITransformer<IEnumerable<SitefinityCatalog>, IEnumerable<ProductCatalog>>
    {
        public string DefaultCatalogName { private get; set; }

        public IEnumerable<ProductCatalog> Execute(IEnumerable<SitefinityCatalog> @from)
        {
            var ucommerceCatalogs = new List<ProductCatalog>();

            foreach (var sitefinityCatalog in @from)
            {
                ucommerceCatalogs.Add(BuildUCommerceCatalogFromSitefinityCatalog(sitefinityCatalog));
            }

            return ucommerceCatalogs;
        }

        private ProductCatalog BuildUCommerceCatalogFromSitefinityCatalog(SitefinityCatalog sfCatalog)
        {
            var uCommerceCatalog = new ProductCatalog
            {
                Name = DefaultCatalogName,
                PriceGroup = new PriceGroup()  // TODO
                {
                    Name = "US"
                },
                ProductCatalogGroup = new ProductCatalogGroup()  // TODO
                {
                    Name = "uCommerce.dk"
                },
                ShowPricesIncludingVAT = true,
                DisplayOnWebSite = true,
                LimitedAccess = false,
                Deleted = false,
                SortOrder = 0
            };

            return uCommerceCatalog;
        }
    }
}
