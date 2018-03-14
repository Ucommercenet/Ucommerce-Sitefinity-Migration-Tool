using System.Configuration;
using MigrationCommandLineRunner.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner.Operations
{
    public class MigrateProductData : IMigrationOperation
    {
        public log4net.ILog Log { private get; set; }

        public IOperation BuildOperation()
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<ProductListFromSitefinity>()
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
