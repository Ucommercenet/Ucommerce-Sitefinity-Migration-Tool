using System;
using System.Linq;
using Dapper;
using MigrationCommon.Data;
using MigrationCommon.Exceptions;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.ServiceWrappers.Configuration;
using timw255.Sitefinity.RestClient.ServiceWrappers.Ecommerce;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Receivers
{
    public class CatalogsFromSitefinity : Configurable, IReceiver<SitefinityCatalogCurrencyCulture>
    {
        public string SitefinityConnectionString { private get; set; }
        public string SitefinitySiteName { private get; set; }
        public string SitefinityBaseUrl { private get; set; }
        public string SitefinityUsername { private get; set; }
        public string SitefinityPassword { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fetch catalog related data from Sitefinity
        /// </summary>
        /// <returns></returns>
        public SitefinityCatalogCurrencyCulture Receive()
        {
            // currently we are defaulting to a single catalog
            Log.Info($"setting Ucommerce default migrated catalog name to {SitefinitySiteName}");
            var catalog = new SitefinityCatalogCurrencyCulture();
            catalog.CatalogName = SitefinitySiteName;

            try
            {
                using (var sf = new SitefinityRestClient(SitefinityUsername, SitefinityPassword, SitefinityBaseUrl))
                {
                    Log.Info("fetching allowed currencies");
                    GetCatalogCurrencies(catalog, sf);

                    Log.Info("fetching allowed cultures");
                    GetCatalogCultures(catalog, sf);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to fetch catalog currencies and cultures from Sitefinity: \n{ex}");
                throw new MigrationException("Unexpected error while fetching catalog currency/culture information from Sitefinity", ex);
            }

            return catalog;
        }

        private void GetCatalogCultures(SitefinityCatalogCurrencyCulture catalog, SitefinityRestClient sf)
        {
            var localizationWrapper = new LocalizationCulturesServiceWrapper(sf);
            var availableCultures = localizationWrapper.GetCultures("", 0, int.MaxValue, "", "");

            if (!availableCultures.Items.Any())
            {
                Log.Error("No cultures returned from Sitefinity");
                throw new MigrationException("There must be at least one culture to import from Sitefinity");
            }

            catalog.Cultures = availableCultures.Items.ToList();
        }

        private void GetCatalogCurrencies(SitefinityCatalogCurrencyCulture catalog, SitefinityRestClient sf)
        {
            var sitefinitySiteId = GetSitefinitySiteId();
            var currencyWrapper = new CurrencyDataServiceWrapper(sf);
            var availableCurrencies = currencyWrapper.GetAllowedCurrenices(sitefinitySiteId);

            if (availableCurrencies.Currencies.Length == 0)
            {
                Log.Error("No currencies returned from Sitefinity");
                throw new MigrationException("There must be at least one currency to import from Sitefinity");
            }

            catalog.AllowedCurrencies = availableCurrencies;
        }

        private Guid GetSitefinitySiteId()
        {
            Guid siteId;
            using (var connection = SqlSessionFactory.Create(SitefinityConnectionString))
            {
                siteId = connection.QueryFirstOrDefault<Guid>($"select id from sf_sites where nme = '{SitefinitySiteName}'");
            }

            return siteId;
        }
    }
}
