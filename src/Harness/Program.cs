using System;
using uCommerce.SfConnector.Adapters.Receivers;
using uCommerce.SfConnector.Transformers;
using uCommerce.uConnector.Adapters.Senders;

namespace Harness
{
    class Program
    {
        // test harness
        static void Main(string[] args)
        {

            var productListFromSitefinity = new ProductListFromSitefinity
            {
                ConnectionString = "server=(local);database=SitefinityCorporateStarterKit; Integrated Security=SSPI;"
            };
            var results = productListFromSitefinity.Receive();

            var transformer = new SfProductListToUcProductList();
            var ucProducts = transformer.Execute(results);

            var sender = new ProductListToUCommerce
            {
                ConnectionString = "server=(local);database = SitecoreUCommerceSitecore_Web; integrated security = SSPI;"
            };
            sender.Send(ucProducts);

            Console.Read();
        }
    }
}
