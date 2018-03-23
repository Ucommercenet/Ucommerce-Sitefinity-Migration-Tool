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
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        private ISession _session;

        /// <summary>
        /// Persist categories and parent/child category relationships to Ucommerce
        /// </summary>
        /// <param name="categories">transformed categories</param>
        public void Send(IEnumerable<Category> categories)
        {
            var newCategories = categories.ToList();
            _session = SessionFactory.Create(ConnectionString);

            try
            {
                WriteCategories(newCategories);
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to write category data to Ucommerce: \n{ex}");
            }

            try
            {
                WriteCategoryRelationships(newCategories);
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to write product definition data to Ucommerce: \n{ex}");
            }

            Log.Info("category migration done.");
        }

        private void WriteCategories(List<Category> newCategories)
        {
            using (var tx = _session.BeginTransaction())
            {
                foreach (var category in newCategories)
                {
                    Log.Info($"adding {category.Name} Ucommerce category");
                    _session.SaveOrUpdate(category);
                }

                tx.Commit();
                _session.Flush();
            }
        }

        private void WriteCategoryRelationships(List<Category> sourceCategories)
        {
            using (var tx = _session.BeginTransaction())
            {
                foreach (var sourceCategory in sourceCategories)
                {
                    AddCategoryChildren(sourceCategory);
                }

                tx.Commit();
                _session.Flush();
            }
        }

        // Add the child lineage for a category
        private void AddCategoryChildren(Category sourceCategory)
        {
            if (sourceCategory.Categories == null) return;

            var categorySitefinityId = sourceCategory.CategoryProperties
                .First(x => x.DefinitionField.Name == "SitefinityId").Value;
            var parentCategory = _session.Query<Category>().FirstOrDefault(
                x => x.CategoryProperties.Count(prop =>
                         prop.DefinitionField.Name == "SitefinityId" && prop.Value == categorySitefinityId) == 1);

            foreach (var tempChildCategory in sourceCategory.Categories)
            {
                var childCategorySitefinityId = tempChildCategory.CategoryProperties
                    .First(x => x.DefinitionField.Name == "SitefinityId").Value;
                var childCategory = _session.Query<Category>().FirstOrDefault(
                    x => x.CategoryProperties.Count(prop =>
                             prop.DefinitionField.Name == "SitefinityId" && prop.Value == childCategorySitefinityId) ==
                         1);

                if (parentCategory == null || childCategory == null) continue;

                parentCategory.AddCategory(childCategory);
                Log.Info($"adding {childCategory.Name} uCommerce child category to {parentCategory.Name}");

                _session.SaveOrUpdate(parentCategory);
            }
        }
    }
}
