using System;
using System.Linq;
using MigrationCommon.Exceptions;
using NHibernate;
using NHibernate.Linq;
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
                WriteCustomSitefinityIdDefinitionField();
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to write catalog data to Ucommerce: \n{ex}");
                throw new MigrationException("A fatal exception occurred trying to write catalog data to Ucommerce", ex);
            }
        }

        /// <summary>
        /// HACK/TODO: This was quickly added after some third party testing discovered that the
        ///       'SitefinityId' DefinitionField is not getting added on the fly in the Taxonomy RTS
        ///       flow.  Refactor this out of here, either making a separate RTS operation/flow for 
        ///       custom property definitions or figure out why UCommerce/nHibernate is not 
        ///       creating new Definition Fields on the fly as part of cascade saves in the Taxonomy flow. 
        /// </summary>
        private void WriteCustomSitefinityIdDefinitionField()
        {
            const string sitefinityUniqueIdPropertyName = "SitefinityId";
            var definitionField = _session.Query<DefinitionField>().FirstOrDefault(x => x.Name == sitefinityUniqueIdPropertyName);

            if (definitionField != null) return;

            using (var tx = _session.BeginTransaction())
            {
                var defaultDefinition = _session.Query<Definition>()
                    .FirstOrDefault(x => x.Name == "Default Category Definition");
                var dataType = _session.Query<DataType>().FirstOrDefault(x => x.TypeName == "ShortText");
                definitionField = new DefinitionField()
                {
                    Name = sitefinityUniqueIdPropertyName,
                    Multilingual = false,
                    Searchable = false,
                    BuiltIn = false,
                    DefaultValue = string.Empty,
                    DisplayOnSite = false,
                    DataType = dataType,
                    Definition = defaultDefinition
                };
                _session.SaveOrUpdate(definitionField);
                tx.Commit();
            }

            _session.Flush();
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
