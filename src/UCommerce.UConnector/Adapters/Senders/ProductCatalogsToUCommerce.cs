using System;
using System.Collections.Generic;
using MigrationCommon.Exceptions;
using NHibernate;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
    public class ProductCatalogsToUCommerce : Configurable, ISender<ProductCatalog>
    {
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        private ISession _session;

        /// <summary>
        /// Persist catalogs to Ucommerce
        /// </summary>
        /// <param name="catalog">transformed catalog</param>
        public void Send(ProductCatalog catalog)
        {
            _session = SessionFactory.Create(ConnectionString);

            try
            {
                WritePriceGroups(catalog);
                WriteCatalog(catalog);
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to write catalog data to Ucommerce: \n{ex}");
                throw new MigrationException("A fatal exception occurred trying to write catalog data to Ucommerce", ex);
            }
        }

        private void WritePriceGroups(ProductCatalog catalog)
        {
            using (var tx = _session.BeginTransaction())
            {
                foreach (var priceGroup in catalog.AllowedPriceGroups)
                {
                    Log.Info($"adding {priceGroup.Name} price group");
                    _session.SaveOrUpdate(priceGroup);
                }

                tx.Commit();
            }
            _session.Flush();
        }

        private void WriteCatalog(ProductCatalog catalog)
        {
            using (var tx = _session.BeginTransaction())
            {
                Log.Info($"adding {catalog.Name} catalog");
                _session.SaveOrUpdate(catalog);

                tx.Commit();
            }
            _session.Flush();
        }
    }
}
