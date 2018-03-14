using System.Collections.Generic;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Adapters.Receivers
{
    public class ProductTypesFromSitefinity : Configurable, IReceiver<IEnumerable<SitefinityProductType>>
    {
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        public IEnumerable<SitefinityProductType> Receive()
        {
            using (var connection = SqlSessionFactory.Create(ConnectionString))
            {
                return connection.Query<SitefinityProductType>("select * from sf_ec_product_type");
            }
        }
    }
}
