﻿using System;
using System.Configuration;
using uCommerce.SfConnector.Configuration;
using uCommerce.SfConnector.Operations;
using uCommerce.SfConnector.Receivers;
using UConnector;
using UConnector.Api.V1;

namespace MigrationCommandLineRunner
{
    public class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Command line runner entry point
        /// </summary>
        private static void Main(string[] args)
        {
            try
            {
                Log.Info("================== Data Migration CommandLine Runner ==================");
                var operationEngine = new OperationEngine();

                Log.Info("******** Verifying Connectivity ********");
                var connectivityTests = new ConnectivityTests(Log);
                if (!connectivityTests.TestConnections())
                {
                    Log.Info("Fatal exception while trying to establish connectivity to systems under migration.");
                    Console.Read();
                    return;
                }
               
                Log.Info("******** Migrating catalog, culture, currency definitions ********");
                var migrateCatalogs = new MigrateCatalogs {Log = Log};
                operationEngine.Execute(migrateCatalogs.BuildOperation());

                Log.Info("******** Migrating taxonomy ********");
                var migrateTaxonomy = new MigrateTaxonomy() { Log = Log };
                operationEngine.Execute(migrateTaxonomy.BuildOperation());

                Log.Info("******** Migrating product types ********");
                var migrateProductTypes = new MigrateProductTypes { Log = Log };
                operationEngine.Execute(migrateProductTypes.BuildOperation());

                Log.Info("******** Migrating core product data ********");
                var migrateProductData = new MigrateProductData { Log = Log };
                operationEngine.Execute(migrateProductData.BuildOperation());

                Log.Info("======================= Data Migration Complete =======================");
                Console.Read();
            }
            catch (Exception ex)
            {
                Log.Error("There was an error while executing the migration.  Details: ", ex);
                Console.Read();
            }
        }
    }
}
