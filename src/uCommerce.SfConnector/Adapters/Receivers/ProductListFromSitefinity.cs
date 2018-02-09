using System.Collections.Generic;
using NHibernate.Linq;
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
            var session = SqlSessionFactory.CreateSession(ConnectionString).OpenSession();
            return session.Query<SitefinityProduct>();
        }
    }
}
