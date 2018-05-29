using System.Configuration;
using MigrationCommandLineRunner.Helpers;
using MigrationCommon.Configuration;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace uCommerce.SfConnector.Operations
{
    public class MigrateProductData : IMigrationOperation
    {
        public log4net.ILog Log { private get; set; }
        public int Take { private get; set; }
        public int Skip { private get; set; }

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
                    .WithOption(x => x.Skip = Skip)
                    .WithOption(x => x.Take = Take)
                    .WithOption(x => x.Log = Log)
                .Transform<SfProductListToUcProductList>()
                    .WithOption(x => x.SitefinitySiteName = MigrationSettings.Settings.SitefinitySiteName)
                    .WithOption(x => x.Log = Log)
                .Send<ProductListToUCommerce>()
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .ToOperation();
        }
    }
}
