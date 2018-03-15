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
        /// Commit new product definitions to the UCommerce database
        /// </summary>
        /// <param name="input">A list of product definitions</param>
        public void Send(IEnumerable<ProductDefinition> sourceDefinitions)
        {
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var sourceDefinition in sourceDefinitions)
                {
                    var destDefinition = _session.Query<ProductDefinition>().SingleOrDefault(a => a.Name == sourceDefinition.Name);
                    if (destDefinition != null) continue;

                    destDefinition = sourceDefinition;

                    Log.Info($"adding {sourceDefinition.Name} product definition");
                    _session.SaveOrUpdate(destDefinition);
                }
                tx.Commit();
            }
            _session.Flush();
        }
    }
}
