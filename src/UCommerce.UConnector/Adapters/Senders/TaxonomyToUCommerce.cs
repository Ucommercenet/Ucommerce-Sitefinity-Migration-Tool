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
    /// Add taxonomy (categories) and relationships to UCommerce
    /// </summary>
    public class TaxonomyToUCommerce : Configurable, ISender<IEnumerable<Category>>
    {
        private ISession _session;

        public void Send(IEnumerable<Category> @from)
        {
            var tempCategories = @from.ToList();
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var tempCategory in tempCategories)
                {
                    var existingCategory = _session.Query<Category>().Where(x => x.Name == tempCategory.Name);
                    if (existingCategory.Any()) continue;

                    var newCategory = PopulateCategory(tempCategory);
                    Console.WriteLine($"......saving category{newCategory.Name}");
                    _session.SaveOrUpdate(newCategory);
                }

                tx.Commit();
            }
            _session.Flush();

            using (var tx = _session.BeginTransaction())
            {
                CreateCategoryRelationships(tempCategories);

                tx.Commit();
            }
            _session.Flush();
        }

        private void CreateCategoryRelationships(List<Category> tempCategories)
        {
            foreach (var tempCategory in tempCategories)
            {
                AddCategoryChildren(tempCategory);
            }
        }

        // Recursively add the child lineage for a category
        private void AddCategoryChildren(Category tempCategory)
        {
            if (tempCategory.Categories == null) return;

            foreach (var tempChildCategory in tempCategory.Categories)
            {
                var parentCategory = _session.Query<Category>().SingleOrDefault(x => x.Name == tempCategory.Name);
                var childCategory = _session.Query<Category>().SingleOrDefault(x => x.Name == tempChildCategory.Name);

                if (parentCategory == null || childCategory == null) continue;

                parentCategory.AddCategory(childCategory);
                Console.WriteLine($"......adding child category {childCategory.Name} to parent {parentCategory.Name}");
                _session.SaveOrUpdate(parentCategory);

                AddCategoryChildren(childCategory);
            }
        }

        private Category PopulateCategory(Category tempCategory)
        {
            var category = new Category
            {
                Name = tempCategory.Name,
                SortOrder = tempCategory.SortOrder,
                DisplayOnSite = tempCategory.DisplayOnSite
            };

            UpdateCategoryAssociations(tempCategory, category);

            return category;
        }

        private void UpdateCategoryAssociations(Category currentCategory, Category newCategory)
        {
            // Category Descriptions
            UpdateCategoryDescription(currentCategory, newCategory);

            // ProductCatalog association
            UpdateProductCatalogAssociation(currentCategory, newCategory);

            // Category Definition
            UpdateCategoryDefinition(currentCategory, newCategory);
        }

        private void UpdateCategoryDefinition(Category currentCategory, Category newCategory)
        {
            newCategory.Definition = _session.Query<Definition>().SingleOrDefault(x => x.Name == currentCategory.Definition.Name);
        }

        private void UpdateProductCatalogAssociation(Category currentCategory, Category newCategory)
        {
            newCategory.ProductCatalog =  _session.Query<ProductCatalog>().SingleOrDefault(x => x.Name == currentCategory.ProductCatalog.Name);
        }

        private void UpdateCategoryDescription(Category currentCategory, Category newCategory)
        {
            var categoryDescription = currentCategory.CategoryDescriptions.FirstOrDefault();
            if (categoryDescription == null) return;

            newCategory.AddCategoryDescription(categoryDescription);
        }

        public string ConnectionString { get; set; }
    }
}
