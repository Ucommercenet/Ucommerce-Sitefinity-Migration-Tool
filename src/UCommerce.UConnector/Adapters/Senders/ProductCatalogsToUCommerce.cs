using System.Collections.Generic;
using NHibernate;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
    public class ProductCatalogsToUCommerce : Configurable, ISender<IEnumerable<ProductCatalog>>
    {    
        private ISession _session;

        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Persist catalogs to Ucommerce
        /// </summary>
        /// <param name="catalogs">transformed catalogs</param>
        public void Send(IEnumerable<ProductCatalog> catalogs)
        {
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var catalog in catalogs)
                {
                    Log.Info($"adding {catalog.Name} catalog");
                    _session.SaveOrUpdate(catalog);
                }

                tx.Commit();
                _session.Flush();
            }
        }
    }
}
