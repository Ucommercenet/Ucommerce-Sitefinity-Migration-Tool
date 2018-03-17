using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using timw255.Sitefinity.RestClient.Model;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfProductTypesToUcProductDefinitions : ITransformer<IEnumerable<ProductTypeViewModel>,
            IEnumerable<ProductDefinition>>
    {
        public string ConnectionString { private get; set; }
        private ISession _session;

        public IEnumerable<ProductDefinition> Execute(IEnumerable<ProductTypeViewModel> sfProductTypes)
        {
            _session = SessionFactory.Create(ConnectionString);
            var productDefinitionList = new List<ProductDefinition>();

            try
            {
                foreach (var sfProductType in sfProductTypes)
                {
                    var definition = _session.Query<ProductDefinition>()
                        .FirstOrDefault(a => a.Name == sfProductType.Title);
                    if (definition != null) continue;

                    productDefinitionList.Add(new ProductDefinition()
                    {
                        Name = sfProductType.Title
                    });
                }
            }
            finally
            {
                _session.Close();
            }

            return productDefinitionList;
        }
    }
}
