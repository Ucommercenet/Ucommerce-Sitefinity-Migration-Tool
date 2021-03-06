﻿using System.Collections.Generic;
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
        public log4net.ILog Log { private get; set; }

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

                    var newDefinition = (new ProductDefinition()
                    {
                        Name = sfProductType.Title
                    });

                    AddInventoryDefinitionField(newDefinition);
                    AddCustomDefinitionFields(newDefinition, sfProductType);
                    productDefinitionList.Add(newDefinition);
                }
            }
            finally
            {
                _session.Close();
            }

            return productDefinitionList;
        }

        public void AddInventoryDefinitionField(ProductDefinition definition)
        {
            var dataType = _session.Query<DataType>().FirstOrDefault(x => x.TypeName == "Number");
            var definitionField = new ProductDefinitionField()
            {
                Multilingual = false,
                DisplayOnSite = true,
                Deleted = false,
                RenderInEditor = true,
                IsVariantProperty = true,
                SortOrder = 0,
                Name = "InventoryOnHand",
                DataType = dataType
            };

            definition.AddProductDefinitionField(definitionField);
        }

        private void AddCustomDefinitionFields(ProductDefinition definition, ProductTypeViewModel sfProductType)
        {
            var dataType = _session.Query<DataType>().FirstOrDefault(x => x.TypeName == "ShortText");
            if (sfProductType.ProductAttributes == null)
            {
                Log.Info($"No product attributes found to associate to product definition '{sfProductType.Title}'");
                return;
            }

            foreach (var attribute in sfProductType.ProductAttributes)
            {
                Log.Info(
                    $"Adding attribute/definition field '{attribute.Title}' to product definition '{definition.Name}'");
                var definitionField = new ProductDefinitionField()
                {
                    Multilingual = true,
                    DisplayOnSite = attribute.Visible,
                    Deleted = false,
                    RenderInEditor = true,
                    IsVariantProperty = true,
                    SortOrder = 0,
                    Name = attribute.Title.Trim(),
                    DataType = dataType
                };

                definition.AddProductDefinitionField(definitionField);
            }
        }
    }
}