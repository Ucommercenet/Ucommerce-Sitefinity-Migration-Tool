using System.Configuration;
using MigrationCommon.Configuration;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace uCommerce.SfConnector.Operations
{
    public class MigrateTaxonomy : IMigrationOperation
    {
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fluent operation for migrating category taxonomy
        /// </summary>
        /// <returns>operation</returns>
        public IOperation BuildOperation()
        {
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<TaxonomyFromSitefinity>()
                    .WithOption(x => x.SitefinityBaseUrl = MigrationSettings.Settings.SitefinityBaseUrl)
                    .WithOption(x => x.SitefinityUsername = MigrationSettings.Settings.SitefinityUsername)
                    .WithOption(x => x.SitefinityPassword = MigrationSettings.Settings.SitefinityPassword)
                    .WithOption(x => x.SitefinityDepartmentTaxonomyId= MigrationSettings.Settings.SitefinityDepartmentTaxonomyId)
                    .WithOption(x => x.Log = Log)
                .Transform<SfTaxonomyToUcTaxonomy>()
                    .WithOption(x => x.DefaultCatalogName = MigrationSettings.Settings.SitefinitySiteName)
                    .WithOption(x => x.DefaultCategoryDefinitionName = MigrationSettings.Settings.DefaultUcommerceCategoryDefinitionName)
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .Send<TaxonomyToUCommerce>()
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .ToOperation();
        }
    }
}
