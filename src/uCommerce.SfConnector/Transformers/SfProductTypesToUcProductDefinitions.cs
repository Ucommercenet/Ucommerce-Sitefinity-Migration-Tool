using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using uCommerce.SfConnector.Model;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class
        SfProductTypesToUcProductDefinitions : ITransformer<IEnumerable<SitefinityProductType>,
            IEnumerable<ProductDefinition>>
    {
        public IEnumerable<ProductDefinition> Execute(IEnumerable<SitefinityProductType> @from)
        {
            var productDefinitionList = new List<ProductDefinition>();
            foreach (var sfProductType in @from)
            {
                productDefinitionList.Add(new ProductDefinition()
                {
                    Name = sfProductType.Title_
                });
            }

            return productDefinitionList;
        }
    }
}
