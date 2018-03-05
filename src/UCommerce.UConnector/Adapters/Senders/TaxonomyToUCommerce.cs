using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using uCommerce.uConnector.Helpers;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
    public class TaxonomyToUCommerce : Configurable, ISender<IEnumerable<Category>>
    {
        private ISession _session;

        public void Send(IEnumerable<Category> input)
        {
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var tempCategory in input)
                {
                    var category = new Category
                    {
                        Name = tempCategory.Name,
                        SortOrder = tempCategory.SortOrder,
                        DisplayOnSite = tempCategory.DisplayOnSite

                    };

                    UpdateCategory(tempCategory, category);
            
                    _session.SaveOrUpdate(category);

                    tx.Commit();

                    _session.Flush();
                }
            }
        }

        private void UpdateCategory(Category currentCategory, Category newCategory)
        {
            // Category Descriptions
            UpdateCategoryDescription(currentCategory, newCategory);

            // ProductCatalog association
            UpdateProductCatalogAssociation(currentCategory, newCategory);

            // Category Definition
            UpdateCategoryDefinition(currentCategory, newCategory);

            // Category associations
        }

        private void UpdateCategoryDefinition(Category currentCategory, Category newCategory)
        {
            newCategory.Definition = _session.Query<Definition>().SingleOrDefault(x => x.Name == "Default Category Definition");
        }

        private void UpdateProductCatalogAssociation(Category currentCategory, Category newCategory)
        {
            newCategory.ProductCatalog =  _session.Query<ProductCatalog>().SingleOrDefault(x => x.Name == "Albert New Catalog");
        }

        private void UpdateCategoryDescription(Category currentCategory, Category newCategory)
        {
            var categoryDescription = currentCategory.CategoryDescriptions.FirstOrDefault();
            if (categoryDescription == null) return;

            newCategory.CategoryDescriptions.Add(categoryDescription);
        }

        public string ConnectionString { get; set; }
    }
}
