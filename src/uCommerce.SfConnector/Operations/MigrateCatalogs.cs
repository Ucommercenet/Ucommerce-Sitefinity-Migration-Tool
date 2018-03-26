using System.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Configuration;
using uCommerce.SfConnector.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace uCommerce.SfConnector.Operations
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
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<CatalogsFromSitefinity>()
                    .WithOption(x => x.SitefinitySiteName = MigrationSettings.Settings.SitefinitySiteName)
                    .WithOption(x => x.SitefinityConnectionString = sitefinityConnectionString)
                    .WithOption(x => x.SitefinityBaseUrl = MigrationSettings.Settings.SitefinityBaseUrl)
                    .WithOption(x => x.SitefinityUsername = MigrationSettings.Settings.SitefinityUsername)
                    .WithOption(x => x.SitefinityPassword = MigrationSettings.Settings.SitefinityPassword)
                    .WithOption(x => x.Log = Log)
                .Transform<SfCatalogsToUcCatalogs>()
                    .WithOption(x => x.DefaultCatalogGroupName = MigrationSettings.Settings.DefaultUcommerceCatalogGroupName)
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .Send<ProductCatalogsToUCommerce>()
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .ToOperation();
        }
    }
}
