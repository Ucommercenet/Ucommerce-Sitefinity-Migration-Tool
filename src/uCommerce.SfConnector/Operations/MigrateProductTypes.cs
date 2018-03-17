﻿using System.Configuration;
using MigrationCommandLineRunner.Helpers;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Configuration;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner.Operations
{
    public class MigrateProductTypes : IMigrationOperation
    {
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fluent operation for migrating product definitions
        /// </summary>
        /// <returns>operation</returns>
        public IOperation BuildOperation()
        {
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            return FluentOperationBuilder
                .Receive<ProductTypesFromSitefinity>()
                    .WithOption(x => x.SitefinityBaseUrl = MigrationSettings.Settings.SitefinityBaseUrl)
                    .WithOption(x => x.SitefinityUsername = MigrationSettings.Settings.SitefinityUsername)
                    .WithOption(x => x.SitefinityPassword = MigrationSettings.Settings.SitefinityPassword)
                    .WithOption(x => x.Log = Log)
                .Transform<SfProductTypesToUcProductDefinitions>()
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                .Send<ProductDefinitionsToUCommerce>()
                    .WithOption(x => x.ConnectionString = uCommerceConnectionString)
                    .WithOption(x => x.Log = Log)
                .ToOperation();
        }
    }
}