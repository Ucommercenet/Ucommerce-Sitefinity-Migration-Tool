using System.Configuration;
using MigrationCommandLineRunner.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner.Operations
{
    public class MigrateTaxonomy : IMigrationOperation
    {
        public IOperation BuildOperation()
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<TaxonomyFromSitefinity>()
                .WithOption(x => x.ConnectionString = sitefinityConnectionString)
                .Transform<SfTaxonomyToUcTaxonomy>()
                    .WithOption(x => x.DefaultCatalogName = MigrationSettings.Settings.DefaultCatalogName)
                    .WithOption(x => x.DefaultCategoryDefinitionName = MigrationSettings.Settings.DefaultCategoryDefinitionName)
                .Send<TaxonomyToUCommerce>()
                .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                .ToOperation();
        }
    }
}
