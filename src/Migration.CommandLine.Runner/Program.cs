using System;
using MigrationCommandLineRunner.Operations;
using UConnector;

namespace MigrationCommandLineRunner
{
    public class Program
    {
        /// <summary>
        /// Command line runner entry point
        /// </summary>
        static void Main(string[] args)
        {
            // NOT WORKING... WHY??

            Console.WriteLine("====Data Migration CommandLine Runner====");
            Console.WriteLine();

            var operationEngine = new OperationEngine();
            Console.WriteLine("... Migrating product types");
            var operation = new MigrateProductTypes().BuildOperation();
            operationEngine.Execute(operation);

            Console.WriteLine("... Migrating core product data");
            operationEngine.Execute(new MigrateProductData().BuildOperation());

            Console.WriteLine();
            Console.WriteLine("=========Data Migration Complete=========");
            Console.Read();
        }
    }
}
