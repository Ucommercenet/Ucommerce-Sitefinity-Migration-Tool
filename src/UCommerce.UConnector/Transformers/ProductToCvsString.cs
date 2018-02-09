using System.Linq;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.uConnector.Transformers
{
	public class ProductToCvsString : ITransformer<Product, string>
	{
		#region ITransformer<Product,string> Members

		public string Execute(Product item)
        {
            return string.Format(@"{0}, ""{1}"", ""{2}""", item.ProductId, item.Name,
                                 string.Join(",", item.GetCategories().Select(x => x.Name).ToArray()));
        }

        #endregion
    }
}