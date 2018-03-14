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
                        ShowPricesIncludingVAT = productCatalog.ShowPricesIncludingVAT,
                        DisplayOnWebSite = productCatalog.DisplayOnWebSite,
                        LimitedAccess = productCatalog.LimitedAccess,
                        Deleted = productCatalog.Deleted,
                        SortOrder = productCatalog.SortOrder
                    };

                    UpdateCatalog(productCatalog, productCatalogDef);

                    Console.WriteLine($"......adding {productCatalog.Name} catalog");
                    _session.SaveOrUpdate(productCatalogDef);
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

            productCatalogGroup = new ProductCatalogGroup()
            {
                CreateCustomersAsMembers = sourceCatalog.ProductCatalogGroup.CreateCustomersAsMembers,
                ProductReviewsRequireApproval = sourceCatalog.ProductCatalogGroup.ProductReviewsRequireApproval,
                Name = sourceCatalog.ProductCatalogGroup.Name,
                Currency = UpdateCurrency(sourceCatalog.PriceGroup.Currency),
                EmailProfile = _session.Query<EmailProfile>().SingleOrDefault(a => a.Name == sourceCatalog.ProductCatalogGroup.EmailProfile.Name)
        };

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

            priceGroup = new PriceGroup()
            {
                Name = sourceCatalog.PriceGroup.Name,
                Description = sourceCatalog.PriceGroup.Description,
                Deleted = sourceCatalog.PriceGroup.Deleted,
                VATRate = sourceCatalog.PriceGroup.VATRate,
            };

            destCatalog.PriceGroup = priceGroup;
            destCatalog.PriceGroup.Currency = UpdateCurrency(sourceCatalog.PriceGroup.Currency);
        }

        private Currency UpdateCurrency(Currency sourceCurrency)
        {
            var currency = _session.Query<Currency>()
                .SingleOrDefault(a => a.ISOCode == sourceCurrency.ISOCode);

            if (currency != null)
            {
                return currency;
            }

            currency = new Currency()
            {
                ISOCode = sourceCurrency.ISOCode,
                Deleted = sourceCurrency.Deleted,
                ExchangeRate = sourceCurrency.ExchangeRate,
                Name = sourceCurrency.Name
            };

            return currency;
        }

        public string ConnectionString { get; set; }
    }
}
