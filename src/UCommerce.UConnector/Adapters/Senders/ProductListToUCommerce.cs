using System;
using System.Collections.Generic;
using MigrationCommon.Extensions;
using NHibernate;
using UCommerce.EntitiesV2;
using uCommerce.uConnector.Helpers;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
	public class ProductListToUCommerce : Configurable, ISender<IEnumerable<Product>>
	{
	    public string ConnectionString { private get; set; }
	    public log4net.ILog Log { private get; set; }

	    private ISession _session;

        /// <summary>
        /// Persist products to Ucommerce
        /// </summary>
        /// <param name="products">transformed products</param>
	    public void Send(IEnumerable<Product> products)
		{
		    _session = SessionFactory.Create(ConnectionString);

		    try
            {
		        WriteProducts(products);
		        Log.Info("product migration done.");
		    }
		    catch (Exception ex)
		    {
		        Log.Fatal($"A fatal exception occurred trying to write product data to Ucommerce: \n{ex}");
		    }
		}

	    private void WriteProducts(IEnumerable<Product> products)
	    {
	        using (var tx = _session.BeginTransaction())
	        {
	            foreach (var product in products)
	            {
	                Log.Info($"adding {product.Name.ToShortName(25)} product");
	                _session.SaveOrUpdate(product);
	            }

	            tx.Commit();
	        }

	        _session.Flush();
	    }
	}
}