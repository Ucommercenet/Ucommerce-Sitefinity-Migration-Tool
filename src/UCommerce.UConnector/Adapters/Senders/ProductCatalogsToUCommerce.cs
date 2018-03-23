using System;
using System.Collections.Generic;
using NHibernate;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
    public class ProductCatalogsToUCommerce : Configurable, ISender<IEnumerable<ProductCatalog>>
    {
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        private ISession _session;

        /// <summary>
        /// Persist catalogs to Ucommerce
        /// </summary>
        /// <param name="catalogs">transformed catalogs</param>
        public void Send(IEnumerable<ProductCatalog> catalogs)
        {
            _session = SessionFactory.Create(ConnectionString);

            try
            {
                WriteCatalogs(catalogs);
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to write catalog data to Ucommerce: \n{ex}");
            }
        }

        private void WriteCatalogs(IEnumerable<ProductCatalog> catalogs)
        {
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
