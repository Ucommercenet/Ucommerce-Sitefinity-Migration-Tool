using System.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Configuration;
using uCommerce.SfConnector.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner.Operations
{
    public class MigrateCatalogs : IMigrationOperation
    {
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fluent operation for migrating catalogs
        /// </summary>
        /// <returns>operation</returns>
        public IOperation BuildOperation()
        {
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<CatalogsFromSitefinity>()
                    .WithOption(x => x.DefaultCatalogName = MigrationSettings.Settings.DefaultUcommerceCatalogName)
                    .WithOption(x => x.Log = Log)
                .Transform<SfCatalogsToUcCatalogs>()
                    .WithOption(x => x.DefaultCatalogGroupName = MigrationSettings.Settings.DefaultUcommerceCatalogGroupName)
                    .WithOption(x => x.DefaultPriceGroupName = MigrationSettings.Settings.DefaultUcommercePriceGroupName)
                    .WithOption(x => x.DefaultCurrencyISOCode = MigrationSettings.Settings.DefaultUcommerceCurrencyISOCode)
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                .Send<ProductCatalogsToUCommerce>()
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .ToOperation();
        }
    }
}
