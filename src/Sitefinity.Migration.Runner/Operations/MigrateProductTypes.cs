using System.Configuration;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace Sitefinity.Migration.Runner.Operations
{
    public class MigrateProductTypes : Operation
    {
        protected override IOperation BuildOperation()
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<ProductTypesFromSitefinity>()
                .WithOption(x => x.ConnectionString = sitefinityConnectionString)
                .Transform<SfProductTypesToUcProductDefinitions>()
                .Send<ProductDefinitionsToUCommerce>()
                .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                .ToOperation();
        }
    }
}
