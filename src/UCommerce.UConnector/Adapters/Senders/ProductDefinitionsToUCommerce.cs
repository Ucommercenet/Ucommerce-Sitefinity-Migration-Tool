using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
    /// <summary>
    /// Write product definitions to uCommerce
    /// </summary>
    public class ProductDefinitionsToUCommerce : Configurable, ISender<IEnumerable<ProductDefinition>>
    {
        private ISession _session;

        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Persist product definitions to Ucommerce
        /// </summary>
        /// <param name="definitions">transformed definitions</param>
        public void Send(IEnumerable<ProductDefinition> definitions)
        {
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var definition in definitions)
                {
                    Log.Info($"adding {definition.Name} Ucommerce product definition");
                    _session.SaveOrUpdate(definition);
                }
                tx.Commit();
            }
            _session.Flush();
            Log.Info("product definition migration done.");
        }
    }
}
