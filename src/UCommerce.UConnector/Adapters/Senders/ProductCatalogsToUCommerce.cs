﻿using System;
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

        public void Send(IEnumerable<ProductCatalog> productCatalogs)
        {
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var productCatalog in productCatalogs)
                {
                    var productCatalogDef = _session.Query<ProductCatalog>()
                        .SingleOrDefault(a => a.Name == productCatalog.Name);
                    if (productCatalogDef != null) continue;

                    productCatalogDef = new ProductCatalog
                    {
                        Name = productCatalog.Name,
                        PriceGroup = _session.Query<PriceGroup>()
                            .SingleOrDefault(a => a.Name == productCatalog.PriceGroup.Name),
                        ProductCatalogGroup = _session.Query<ProductCatalogGroup>()
                            .SingleOrDefault(a => a.Name == productCatalog.ProductCatalogGroup.Name),
                        ShowPricesIncludingVAT = productCatalog.ShowPricesIncludingVAT,
                        DisplayOnWebSite = productCatalog.DisplayOnWebSite,
                        LimitedAccess = productCatalog.LimitedAccess,
                        Deleted = productCatalog.Deleted,
                        SortOrder = productCatalog.SortOrder
                    };

                    Console.WriteLine($"......adding {productCatalog.Name} catalog");
                    _session.SaveOrUpdate(productCatalogDef);
                }

                tx.Commit();
            }
            _session.Flush();
        }

        public string ConnectionString { get; set; }
    }
}