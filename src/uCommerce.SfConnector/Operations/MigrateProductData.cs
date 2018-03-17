using System.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Configuration;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner.Operations
{
    public class MigrateProductData : IMigrationOperation
    {
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fluent operation for migrating product data
        /// </summary>
        /// <returns>operation</returns>
        public IOperation BuildOperation()
        {
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<ProductListFromSitefinity>()
                    .WithOption(x => x.SitefinityBaseUrl = MigrationSettings.Settings.SitefinityBaseUrl)
                    .WithOption(x => x.SitefinityUsername = MigrationSettings.Settings.SitefinityUsername)
                    .WithOption(x => x.SitefinityPassword = MigrationSettings.Settings.SitefinityPassword)
                    .WithOption(x => x.ConnectionString = sitefinityConnectionString)
                    .WithOption(x => x.Log = Log)
                .Transform<SfProductListToUcProductList>()
                    .WithOption(x => x.DefaultPriceGroupName = MigrationSettings.Settings.DefaultUcommercePriceGroupName)
                .Send<ProductListToUCommerce>()
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .ToOperation();
        }
    }
}
