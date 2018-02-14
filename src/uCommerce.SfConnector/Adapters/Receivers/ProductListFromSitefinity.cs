using System.Collections.Generic;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Adapters.Receivers
{
    public class ProductListFromSitefinity : Configurable, IReceiver<IEnumerable<SitefinityProduct>>
    {
        public string ConnectionString { private get; set; }

        public IEnumerable<SitefinityProduct> Receive()
        {
            using (var connection = SqlSessionFactory.Create(ConnectionString))
            {
                return connection.Query<SitefinityProduct>("select * from sf_ec_product");
            }
        }
    }
}
