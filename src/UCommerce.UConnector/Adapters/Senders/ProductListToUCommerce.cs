using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using UCommerce.EntitiesV2;
using uCommerce.uConnector.Helpers;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
	public class ProductListToUCommerce : Configurable, ISender<IEnumerable<Product>>
	{
		private ISession _session;

	    public string ConnectionString { private get; set; }
	    public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Persist products to Ucommerce
        /// </summary>
        /// <param name="products">transformed products</param>
	    public void Send(IEnumerable<Product> products)
		{
			_session = SessionFactory.Create(ConnectionString);

			using (var tx = _session.BeginTransaction())
			{
			    foreach (var product in products)
			    {
			        Log.Info($"adding {AbridgedName(product.Name)} product");
			        _session.SaveOrUpdate(product);
			    }
				tx.Commit();
			}
			_session.Flush();
		    Log.Info("product migration done.");
        }

	    private string AbridgedName(string name)
	    {
 	        if (name.Length > 25)
	        {
	            return name.Substring(0, 25) + "...";
	        }

	        return name;
	    }
	}
}