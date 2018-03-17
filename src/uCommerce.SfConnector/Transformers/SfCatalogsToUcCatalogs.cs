using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using uCommerce.SfConnector.Model;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfCatalogsToUcCatalogs : ITransformer<IEnumerable<SitefinityCatalog>, IEnumerable<ProductCatalog>>
    {
        public string DefaultCatalogGroupName { private get; set; }
        public string DefaultPriceGroupName { private get; set; }
        public string DefaultCurrencyISOCode { private get; set; }
        public string ConnectionString { private get; set; }

        private ISession _session;

        public IEnumerable<ProductCatalog> Execute(IEnumerable<SitefinityCatalog> catalogs)
        {
            _session = SessionFactory.Create(ConnectionString);
            var ucommerceCatalogs = new List<ProductCatalog>();

            try
            {
                foreach (var sitefinityCatalog in catalogs) 
                {
                    ucommerceCatalogs.Add(BuildUCommerceCatalogFromSitefinityCatalog(sitefinityCatalog));
                }
            }
            finally
            {
                _session.Close();
            }

            return ucommerceCatalogs;
        }

        private ProductCatalog BuildUCommerceCatalogFromSitefinityCatalog(SitefinityCatalog sfCatalog)
        {
            var uCommerceCatalog = _session.Query<ProductCatalog>().FirstOrDefault(a => a.Name == sfCatalog.CatalogName);

            if (uCommerceCatalog != null) return uCommerceCatalog;

            var currency = CreateCurrency(DefaultCurrencyISOCode);
            var priceGroup = CreatePriceGroup(DefaultPriceGroupName, currency);
            var productCatalogGroup = CreateProductCatalogGroup(DefaultCatalogGroupName, currency);
            uCommerceCatalog = new ProductCatalog
            {
                Name = sfCatalog.CatalogName,
                PriceGroup = priceGroup,
                ProductCatalogGroup = productCatalogGroup,
                ShowPricesIncludingVAT = true,
                DisplayOnWebSite = true,
                LimitedAccess = false,
                Deleted = false,
                SortOrder = 0
            };

            return uCommerceCatalog;
        }

        private ProductCatalogGroup CreateProductCatalogGroup(string catalogGroupName, Currency currency)
        {
            return new ProductCatalogGroup()
            {
                Name = catalogGroupName,
                Currency = currency,
                CreateCustomersAsMembers = true,
                ProductReviewsRequireApproval = true,
                Deleted = false,
                EmailProfile = _session.Query<EmailProfile>()
                    .SingleOrDefault(a => a.Name == "Default")
            };
        }

        private PriceGroup CreatePriceGroup(string priceGroupName, Currency priceGroupCurrency)
        {
            var priceGroup = _session.Query<PriceGroup>().SingleOrDefault(a => a.Name == priceGroupName);
            if (priceGroup != null)
            {
                return priceGroup;
            }

            return new PriceGroup()
            {
                Name = priceGroupName,
                Deleted = false,
                Currency = priceGroupCurrency
            };
        }

        private Currency CreateCurrency(string ISOCode)
        {
            var currency = _session.Query<Currency>()
                .SingleOrDefault(a => a.ISOCode == ISOCode);

            if (currency != null)
            {
                return currency;
            }

            return new Currency()
            {  
                ISOCode = ISOCode,
                ExchangeRate = 0,
                Deleted = false
            };
        }
    }
}