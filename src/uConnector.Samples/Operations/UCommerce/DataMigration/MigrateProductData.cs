using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Receivers;
using uCommerce.uConnector.Adapters.Senders;
using uCommerce.uConnector.Transformers;
using UConnector.Api.V1;
using UConnector.IO;
using UConnector.IO.Csv;

namespace UConnector.Samples.Operations.UCommerce.DataMigration
{
    public class MigrateProductData : Operation
    {
        protected override IOperation BuildOperation()
        {
            return FluentOperationBuilder
                .Receive<ProductListFromSitefinity>()
                    .WithOption(x => x.ConnectionString = "server=(local);database=SitefinityCorporateStarterKit;integrated security=SSPI;")
                .Transform<SfProductListToUcProductList>()
                .Send<ProductListToUCommerce>()
                .WithOption(x => x.ConnectionString = "server=(local);database=SitecoreUCommerceSitecore_Web;integrated security=SSPI;")
                .ToOperation();
        }
    }
}
