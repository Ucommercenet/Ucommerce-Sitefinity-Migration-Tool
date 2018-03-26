using System.Linq;
using NHibernate;
using NHibernate.Linq;
using timw255.Sitefinity.RestClient.Model;
using uCommerce.SfConnector.Model;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfCatalogsToUcCatalogs : ITransformer<SitefinityCatalogCurrencyCulture, ProductCatalog>
    {
        public string DefaultCatalogGroupName { private get; set; }
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        private ISession _session;

        public ProductCatalog Execute(SitefinityCatalogCurrencyCulture catalog)
        {
            _session = SessionFactory.Create(ConnectionString);

            try
            {
                return BuildUCommerceCatalogFromSitefinityCatalog(catalog);
            }
            finally
            {
                _session.Close();
            }
        }

        private ProductCatalog BuildUCommerceCatalogFromSitefinityCatalog(SitefinityCatalogCurrencyCulture sfCatalogCurrencyCulture)
        {
            var uCommerceCatalog = _session.Query<ProductCatalog>().FirstOrDefault(a => a.Name == sfCatalogCurrencyCulture.CatalogName);

            if (uCommerceCatalog != null)
            {
                Log.Warn($"Catalog name {sfCatalogCurrencyCulture.CatalogName} already present in UCommerce");
                return uCommerceCatalog;
            }

            return CreateCatalog(sfCatalogCurrencyCulture);
        }

        private ProductCatalog CreateCatalog(SitefinityCatalogCurrencyCulture sfCatalogCurrencyCulture)
        {
            var uCommerceCatalog = new ProductCatalog();
            var defaultCurrency = AddCatalogPriceGroups(uCommerceCatalog, sfCatalogCurrencyCulture);
            var productCatalogGroup = CreateProductCatalogGroup(DefaultCatalogGroupName, defaultCurrency);

            uCommerceCatalog.Name = sfCatalogCurrencyCulture.CatalogName;
            uCommerceCatalog.ProductCatalogGroup = productCatalogGroup;
            uCommerceCatalog.ShowPricesIncludingVAT = true;
            uCommerceCatalog.DisplayOnWebSite = true;
            uCommerceCatalog.LimitedAccess = false;
            uCommerceCatalog.Deleted = false;
            uCommerceCatalog.SortOrder = 0;

            return uCommerceCatalog;
        }

        private Currency AddCatalogPriceGroups(ProductCatalog uCommerceCatalog, SitefinityCatalogCurrencyCulture sfCatalogCurrencyCulture)
        {
            var defaultCurrency = new Currency();
   
            foreach (var sfCurrency in sfCatalogCurrencyCulture.AllowedCurrencies.Currencies)
            {
                var currentCurrency = CreateCurrency(sfCurrency);
                var priceGroup = new PriceGroup()
                {
                    Name = sfCurrency.DisplayName,
                    Deleted = false,
                    VATRate = sfCurrency.ExchangeRate,
                    Currency = currentCurrency,
                };
     
                if (sfCurrency.IsDefault)
                {
                    uCommerceCatalog.PriceGroup = priceGroup;
                    defaultCurrency = currentCurrency;
                }

                uCommerceCatalog.AddAllowedPriceGroup(priceGroup);
            }

            return defaultCurrency;
        }

        private ProductCatalogGroup CreateProductCatalogGroup(string catalogGroupName, Currency defaultCurrency)
        {
            var productCatalogGroup = new ProductCatalogGroup()
            {
                Name = catalogGroupName,
                Currency = defaultCurrency,
                CreateCustomersAsMembers = true,
                ProductReviewsRequireApproval = true,
                Deleted = false,
                EmailProfile = _session.Query<EmailProfile>()
                    .SingleOrDefault(a => a.Name == "Default")
            };
        
            return productCatalogGroup;
        }

        private Currency CreateCurrency(CurrencyViewModel currency)
        {
            var existingCurrency = _session.Query<Currency>()
                .SingleOrDefault(a => a.ISOCode == currency.Key);

            if (existingCurrency != null)
            {
                return existingCurrency;
            }

            return new Currency()
            {  
                ISOCode = currency.Key,
                ExchangeRate = 0,
                Deleted = false,
            };
        }
    }
}