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
        public void Send(IEnumerable<ProductDefinition> productDefinitions)
        {
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var tempProductDef in productDefinitions)
                {
                    var productDef = _session.Query<ProductDefinition>().SingleOrDefault(a => a.Name == tempProductDef.Name);
                    if (productDef == null) 
                    {
                        productDef = new ProductDefinition
                        {
                            Name = tempProductDef.Name
                        };

                        Log.Info($"adding {tempProductDef.Name} product definition");
                        _session.SaveOrUpdate(productDef);
                    }
                }
                tx.Commit();
            }
            _session.Flush();
        }
    }
}
