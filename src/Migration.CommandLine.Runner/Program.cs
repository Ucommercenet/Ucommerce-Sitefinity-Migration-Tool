using System;
using MigrationCommon.Data;
using uCommerce.SfConnector.Operations;
using UConnector;

namespace MigrationCommandLineRunner
{
    public class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const int ProductBatchSize = 25;

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
                    Log.Fatal("Fatal exception while trying to establish connectivity to systems under migration.");
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
                var productCount = DataHelper.GetProductCount();
                var currentBatchCount = 0;
                do
                {
                    var migrateProductData = new MigrateProductData {Log = Log, Skip = currentBatchCount, Take = ProductBatchSize};
                    operationEngine.Execute(migrateProductData.BuildOperation());
                    currentBatchCount += ProductBatchSize;
                } while (currentBatchCount < productCount);

                Log.Info("======================= Data Migration Complete =======================");
                Console.Read();
            }
            catch (Exception ex)
            {
                Log.Fatal("There was an unrecoverable error while executing the migration.  Details: ", ex);
                Console.Read();
            }
        }
    }
}
