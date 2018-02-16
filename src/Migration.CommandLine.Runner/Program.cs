using System;
using System.Configuration;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;

namespace Harness
{
    public class Program
    {
        public static string SitefinityConnectionString;
        public static string UCommerceConnectionString;
        
        /// <summary>
        /// Command line runner entry point
        /// </summary>
        static void Main(string[] args)
        {
            //TODO: Create migration log file
            //TODO: Exception handling
            Console.WriteLine("====Data Migration CommandLine Runner====");

            SitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            UCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;

            CreateMigrationHelperData();

            MigrateProductTypes();
            MigrateProductData();

            Console.WriteLine("=========Data Migration Complete=========");
            Console.Read();
        }

        private static void CreateMigrationHelperData()
        {
            Console.WriteLine("... Creating migration helper entites and data");
            CreateTypesToProductsMigrationData.PopulateData(SitefinityConnectionString);
        }

        private static void MigrateProductData()
        {
            //TODO: perhaps more Fluent execution of tasks?

            Console.WriteLine("... Fetching product data from Sitefinity");
            var productListFromSitefinity = new ProductListFromSitefinity
            {
                ConnectionString = SitefinityConnectionString
            };
            var results = productListFromSitefinity.Receive();

            Console.WriteLine("... Transforming Sitefinity products to UCommerce products");
            var transformer = new SfProductListToUcProductList();
            var ucProducts = transformer.Execute(results);

            Console.WriteLine("... Writing products to UCommerce");
            var sender = new ProductListToUCommerce
            {
                ConnectionString = UCommerceConnectionString
            };
            sender.Send(ucProducts);
        }

        private static void MigrateProductTypes()
        {
            Console.WriteLine("... Fetching product type data from Sitefinity");
            var productTypesFromSitefinity = new ProductTypesFromSitefinity
            {
                ConnectionString = SitefinityConnectionString
            };
            var results = productTypesFromSitefinity.Receive();

            Console.WriteLine("... Transforming Sitefinity product types to UCommerce product definitions");
            var transformer = new SfProductTypesToUcProductDefinitions();
            var ucProductDefinitions = transformer.Execute(results);

            Console.WriteLine("... Writing product definitions to UCommerce");
            var sender = new ProductDefinitionsToUCommerce() {
                ConnectionString = UCommerceConnectionString
            };
            sender.Send(ucProductDefinitions);
        }
    }
}
