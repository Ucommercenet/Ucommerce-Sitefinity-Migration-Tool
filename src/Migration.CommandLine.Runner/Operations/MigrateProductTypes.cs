using System.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner.Operations
{
    public class MigrateProductTypes : IMigrationOperation
    {
        public IOperation BuildOperation()
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<ProductTypesFromSitefinity>()
                .WithOption(x => x.ConnectionString = sitefinityConnectionString)
                .Batch()
                .Transform<SfProductTypesToUcProductDefinitions>()
                .Send<ProductDefinitionsToUCommerce>()
                .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                .ToOperation();
        }
    }
}
