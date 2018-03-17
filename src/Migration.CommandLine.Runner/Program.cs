using System;
using MigrationCommandLineRunner.Helpers;
using MigrationCommandLineRunner.Operations;
using UConnector;

namespace MigrationCommandLineRunner
{
    public class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Command line runner entry point
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                Log.Info("======== Data Migration CommandLine Runner ========");
                var operationEngine = new OperationEngine();

                Log.Info("******** Migrating catalogs ********");
                var migrateCatalogs = new MigrateCatalogs {Log = Log};
                operationEngine.Execute(migrateCatalogs.BuildOperation());

                Log.Info("******** Migrating taxonomy ********");
                var migrateTaxonomy = new MigrateTaxonomy() {Log = Log};
                operationEngine.Execute(migrateTaxonomy.BuildOperation());

                Log.Info("******** Migrating product types ********");
                var migrateProductTypes = new MigrateProductTypes {Log = Log};
                operationEngine.Execute(migrateProductTypes.BuildOperation());

                Log.Info("******** Migrating core product data ********");
                var migrateProductData = new MigrateProductData {Log = Log};
                operationEngine.Execute(migrateProductData.BuildOperation());

                Log.Info("========= Data Migration Complete =========");
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
