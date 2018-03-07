﻿using System.Configuration;
using MigrationCommandLineRunner.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner.Operations
{
    public class MigrateCatalogs : IMigrationOperation
    {
        public IOperation BuildOperation()
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<CatalogsFromSitefinity>()
                    .WithOption(x => x.ConnectionString = sitefinityConnectionString)
                .Transform<SfCatalogsToUcCatalogs>()
                    .WithOption(x => x.DefaultCatalogName = MigrationSettings.Settings.DefaultCatalogName)
                .Send<ProductCatalogsToUCommerce>()
                .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                .ToOperation();
        }
    }
}