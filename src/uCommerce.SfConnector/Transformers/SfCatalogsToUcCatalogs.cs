using System.Collections.Generic;
using uCommerce.SfConnector.Model;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfCatalogsToUcCatalogs : ITransformer<IEnumerable<SitefinityCatalog>, IEnumerable<ProductCatalog>>
    {
        public string DefaultCatalogName { private get; set; }
        public string DefaultCatalogGroupName { private get; set; }
        public string DefaultPriceGroupName { private get; set; }
        public string DefaultCurrencyISOCode { private get; set; }

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
                PriceGroup = new PriceGroup() 
                {
                    Name = DefaultPriceGroupName,
                    Deleted = false,
                    Currency = CreateDefaultCurrency()
                },
                ProductCatalogGroup = new ProductCatalogGroup() 
                {
                    Name = DefaultCatalogGroupName,
                    Currency = CreateDefaultCurrency(),
                    CreateCustomersAsMembers = true,
                    ProductReviewsRequireApproval = true,
                    Deleted = false,
                    EmailProfile = new EmailProfile()
                    {
                        Name = "Default"
                    }
                },

                ShowPricesIncludingVAT = true,
                DisplayOnWebSite = true,
                LimitedAccess = false,
                Deleted = false,
                SortOrder = 0
            };

            return uCommerceCatalog;
        }

        private Currency CreateDefaultCurrency()
        {
            return new Currency()
            {  
                ISOCode = DefaultCurrencyISOCode,
                ExchangeRate = 0,
                Deleted = false
            };
        }
    }
}