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
        public string ConnectionString { get; set; }

        private ISession _session;

        public void Send(IEnumerable<Category> @from)
        {
            var sourceCategories = @from.ToList();
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var sourceCategory in sourceCategories)
                {
                    var destCategory = PopulateCategory(sourceCategory);
                    Console.WriteLine($"......adding {destCategory.Name} category");
                    _session.SaveOrUpdate(destCategory);
                }

                tx.Commit();
            }
            _session.Flush();

            using (var tx = _session.BeginTransaction())
            {
                CreateCategoryRelationships(sourceCategories);

                tx.Commit();
            }
            _session.Flush();
        }

        private void CreateCategoryRelationships(List<Category> sourceCategories)
        {
            foreach (var sourceCategory in sourceCategories)
            {
                AddCategoryChildren(sourceCategory);
            }
        }

        // Add the child lineage for a category
        private void AddCategoryChildren(Category sourceCategory)
        {
            if (sourceCategory.Categories == null) return;

            var categorySitefinityId = sourceCategory.CategoryProperties.First(x => x.DefinitionField.Name == "SitefinityId").Value;
            var parentCategory = _session.Query<Category>().SingleOrDefault(
                x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == categorySitefinityId) == 1);

            foreach (var tempChildCategory in sourceCategory.Categories)
            {
                var childCategorySitefinityId = tempChildCategory.CategoryProperties.First(x => x.DefinitionField.Name == "SitefinityId").Value;
                var childCategory = _session.Query<Category>().SingleOrDefault(
                    x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == childCategorySitefinityId) == 1);

                if (parentCategory == null || childCategory == null) continue;

                parentCategory.AddCategory(childCategory);
                Console.WriteLine($"......adding {childCategory.Name} child category to {parentCategory.Name}");
                _session.SaveOrUpdate(parentCategory);
            }
        }

        private Category PopulateCategory(Category sourceCategory)
        {
            var category = new Category
            {
                Name = sourceCategory.Name,
                SortOrder = sourceCategory.SortOrder,
                DisplayOnSite = sourceCategory.DisplayOnSite
            };

            UpdateCategoryAssociations(sourceCategory, category);

            return category;
        }

        private void UpdateCategoryAssociations(Category sourceCategory, Category destCategory)
        {
            // Category Descriptions
            UpdateCategoryDescription(sourceCategory, destCategory);
            // ProductCatalog association
            UpdateProductCatalogAssociation(sourceCategory, destCategory);
            // Category Definition
            UpdateCategoryDefinition(sourceCategory, destCategory);
            // Category Custom Properties/Definitions
            UpdateCategoryProperties(sourceCategory, destCategory);
        }

        private void UpdateCategoryProperties(Category sourceCategory, Category destCategory)
        {
            foreach (var property in sourceCategory.CategoryProperties)
            {
                destCategory.CategoryProperties.Add(new CategoryProperty()
                {
                    DefinitionField = GetCategoryPropertyDefinitionField(property.DefinitionField),
                    Value = property.Value,
                    CultureCode = property.CultureCode,
                    Category = destCategory
                });
            }
        }

        private DefinitionField GetCategoryPropertyDefinitionField(DefinitionField sourceDefinitionField)
        {
            var definitionField = _session.Query<DefinitionField>().FirstOrDefault(x => x.Name == sourceDefinitionField.Name);

            if (definitionField != null)
            {
                return definitionField;
            }

            var defaultDefinition = _session.Query<Definition>().FirstOrDefault(x => x.Name == "Default Category Definition");
            var dataType = _session.Query<DataType>().FirstOrDefault(x => x.TypeName == "ShortText");
            definitionField =  new DefinitionField()
            {
                Name = sourceDefinitionField.Name,
                Multilingual = sourceDefinitionField.Multilingual,
                Searchable = sourceDefinitionField.Searchable,
                BuiltIn = sourceDefinitionField.BuiltIn,
                DefaultValue = sourceDefinitionField.DefaultValue,
                DisplayOnSite = sourceDefinitionField.DisplayOnSite,
                DataType = dataType,
                Definition = defaultDefinition
            };

            _session.SaveOrUpdate(definitionField);

            return definitionField;
        }

        private void UpdateCategoryDefinition(Category sourceCategory, Category destCategory)
        {
            destCategory.Definition = _session.Query<Definition>().SingleOrDefault(x => x.Name == sourceCategory.Definition.Name);
        }

        private void UpdateProductCatalogAssociation(Category sourceCategory, Category destCategory)
        {
            destCategory.ProductCatalog =  _session.Query<ProductCatalog>().SingleOrDefault(x => x.Name == sourceCategory.ProductCatalog.Name);
        }

        private void UpdateCategoryDescription(Category sourceCategory, Category destCategory)
        {
            var categoryDescription = sourceCategory.CategoryDescriptions.FirstOrDefault();
            if (categoryDescription == null) return;

            destCategory.AddCategoryDescription(categoryDescription);
        }
    }
}
