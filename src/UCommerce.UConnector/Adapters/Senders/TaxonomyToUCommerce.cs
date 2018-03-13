using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Hql.Ast.ANTLR.Tree;
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
            var tempCategories = @from.ToList();
            _session = SessionFactory.Create(ConnectionString);

            using (var tx = _session.BeginTransaction())
            {
                foreach (var tempCategory in tempCategories)
                {
                    var newCategory = PopulateCategory(tempCategory);
                    Console.WriteLine($"......adding {newCategory.Name} category");
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

        // Add the child lineage for a category
        private void AddCategoryChildren(Category tempCategory)
        {
            if (tempCategory.Categories == null) return;

            var categorySitefinityId = tempCategory.CategoryProperties.First(x => x.DefinitionField.Name == "SitefinityId").Value;
            var parentCategory = _session.Query<Category>().SingleOrDefault(
                x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == categorySitefinityId) == 1);

            foreach (var tempChildCategory in tempCategory.Categories)
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
            // Category Custom Properties/Definitions
            UpdateCategoryProperties(currentCategory, newCategory);
        }

        private void UpdateCategoryProperties(Category currentCategory, Category newCategory)
        {
            foreach (var property in currentCategory.CategoryProperties)
            {
                newCategory.CategoryProperties.Add(new CategoryProperty()
                {
                    DefinitionField = GetCategoryPropertyDefinitionField(property.DefinitionField),
                    Value = property.Value,
                    CultureCode = property.CultureCode,
                    Category = newCategory
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
            // TODO: property associated to wrong Dessert category in UCommerce

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
    }
}
