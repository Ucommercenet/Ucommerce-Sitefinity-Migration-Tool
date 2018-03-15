using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
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

        public void Send(IEnumerable<ProductCatalog> sourceCatalogs)
        {
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var sourceCatalog in sourceCatalogs)
                {
                    var destCatalog = _session.Query<ProductCatalog>()
                        .SingleOrDefault(a => a.Name == sourceCatalog.Name);
                    if (destCatalog != null) continue;

                    destCatalog = sourceCatalog;

                    UpdateCatalog(sourceCatalog, destCatalog);

                    Log.Info($"adding {sourceCatalog.Name} catalog");
                    _session.SaveOrUpdate(destCatalog);
                }

                tx.Commit();
            }
            _session.Flush();
        }

        private void UpdateCatalog(ProductCatalog sourceCatalog, ProductCatalog destCatalog)
        {
            UpdatePriceGroup(sourceCatalog, destCatalog);
            UpdateProductCatalogGroup(sourceCatalog, destCatalog);
        }

        private void UpdateProductCatalogGroup(ProductCatalog sourceCatalog, ProductCatalog destCatalog)
        {
            var productCatalogGroup = _session.Query<ProductCatalogGroup>()
                .SingleOrDefault(a => a.Name == sourceCatalog.ProductCatalogGroup.Name);

            if (productCatalogGroup != null)
            {
                destCatalog.ProductCatalogGroup = productCatalogGroup;
                return;
            }

            productCatalogGroup = destCatalog.ProductCatalogGroup;
            productCatalogGroup.Currency = UpdateCurrency(sourceCatalog.PriceGroup.Currency);
            productCatalogGroup.EmailProfile = _session.Query<EmailProfile>()
                .SingleOrDefault(a => a.Name == sourceCatalog.ProductCatalogGroup.EmailProfile.Name);

            destCatalog.ProductCatalogGroup = productCatalogGroup;
        }

        private void UpdatePriceGroup(ProductCatalog sourceCatalog, ProductCatalog destCatalog)
        {
            var priceGroup = _session.Query<PriceGroup>().SingleOrDefault(a => a.Name == sourceCatalog.PriceGroup.Name);
            if (priceGroup != null)
            {
                destCatalog.PriceGroup = priceGroup;
                return;
            }

            priceGroup = sourceCatalog.PriceGroup;
            priceGroup.Currency = UpdateCurrency(sourceCatalog.PriceGroup.Currency);

            destCatalog.PriceGroup = priceGroup;
        }

        private Currency UpdateCurrency(Currency sourceCurrency)
        {
            var currency = _session.Query<Currency>()
                .SingleOrDefault(a => a.ISOCode == sourceCurrency.ISOCode);

            return currency != null ? currency : sourceCurrency;
        }
    }
}
