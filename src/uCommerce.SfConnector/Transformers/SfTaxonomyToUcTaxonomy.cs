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
    public class SfTaxonomyToUcTaxonomy : ITransformer<IEnumerable<WcfHierarchicalTaxon>, IEnumerable<Category>>
    {
        public string DefaultCatalogName { private get; set; }
        public string DefaultCategoryDefinitionName { private get; set; }
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        private ISession _session;
        private readonly string SitefinityUniqueIdPropertyName = "SitefinityId";

        public IEnumerable<Category> Execute(IEnumerable<WcfHierarchicalTaxon> departments)
        {
            _session = SessionFactory.Create(ConnectionString);
            var sfCategories = departments.ToList();
            var categories = new List<Category>();

            try
            {
                foreach (var sfCategory in sfCategories)
                {
                    var category = BuildCategory(sfCategory);
                    if (category == null) continue;

                    categories.Add(BuildCategory(sfCategory));
                }

                BuildParentChildCategoryRelationships(categories, sfCategories);
            }
            finally
            {
                _session.Close();
            }

            return categories;
        }

        private Category BuildCategory(WcfHierarchicalTaxon sfCategory)
        {
            var category = _session.Query<Category>().FirstOrDefault(
                x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == SitefinityUniqueIdPropertyName && prop.Value == sfCategory.Id.ToString()) == 1);

            if (category != null)
            {
                Log.Warn($"category '{sfCategory.Name}' with Sitefinity id '{sfCategory.Id}' already exists in Ucommerce");
                return null;
            }

            var currentCatalog = GetCatalogAssociation(DefaultCatalogName);
            var categoryDefinition = GetCategoryDefinition();
            category = new Category
            {
                Name = sfCategory.Title,
                ProductCatalog = currentCatalog,
                Definition = categoryDefinition,
                DisplayOnSite = true
            };

            UpdateCategoryAssociations(category, sfCategory);

            return category;
        }

        private void UpdateCategoryAssociations(Category category, WcfHierarchicalTaxon sfCategory)
        {
            // Category Descriptions
            AddCategoryDescriptions(category, sfCategory);

            // Category Custom Properties/Definitions
            AddCategoryProperties(category, sfCategory);
        }

        private void AddCategoryProperties(Category category, WcfHierarchicalTaxon sfCategory)
        {
            var propertyDefField = GetCategoryPropertyDefinitionField("SitefinityId");
            category.AddCategoryProperty(new CategoryProperty()
            {
                DefinitionField = propertyDefField,
                Value = sfCategory.Id.ToString(),
                CultureCode = sfCategory.CultureCode,
                Category = category
            });
        }

        private DefinitionField GetCategoryPropertyDefinitionField(string sourceDefinitionFieldName)
        {
            var definitionField = _session.Query<DefinitionField>().FirstOrDefault(x => x.Name == sourceDefinitionFieldName);

            if (definitionField != null)
            {
                return definitionField;
            }

            var defaultDefinition = _session.Query<Definition>().FirstOrDefault(x => x.Name == "Default Category Definition");
            var dataType = _session.Query<DataType>().FirstOrDefault(x => x.TypeName == "ShortText");
            definitionField = new DefinitionField()
            {
                Name = sourceDefinitionFieldName,
                Multilingual = true,
                Searchable = false,
                BuiltIn = false,
                DefaultValue = string.Empty,
                DisplayOnSite = false,
                DataType = dataType,
                Definition = defaultDefinition
            };

            return definitionField;
        }

        private void AddCategoryDescriptions(Category category, WcfHierarchicalTaxon sfCategory)
        {
            // Category description(s)
            category.CategoryDescriptions.Add(new CategoryDescription()
            {
                CultureCode = sfCategory.CultureCode,
                Description = sfCategory.Description.Trim(),
                DisplayName = sfCategory.Title.Trim(),
                RenderAsContent = true,
                Category = category
            });

            foreach (var translation in sfCategory.CultureTranslations)
            {
                category.AddCategoryDescription(new CategoryDescription()
                {
                    CultureCode = translation.Key,
                    Description = translation.Value.Description.Trim(),
                    DisplayName = translation.Value.Title.Trim(),
                    Category = category
                });
            }
        }

        private Definition GetCategoryDefinition()
        {
            return _session.Query<Definition>().SingleOrDefault(x => x.Name == DefaultCategoryDefinitionName);
        }

        private ProductCatalog GetCatalogAssociation(string catalogName)
        {
            return _session.Query<ProductCatalog>().FirstOrDefault(x => x.Name == catalogName);
        }

        private void BuildParentChildCategoryRelationships(List<Category> categories, List<WcfHierarchicalTaxon> sfCategories)
        {
            foreach (var sfCategory in sfCategories)
            {
                BuildCategoryRelationship(categories, sfCategories, sfCategory);
            }
        }

        private void BuildCategoryRelationship(List<Category> category, List<WcfHierarchicalTaxon> sfCategories, WcfHierarchicalTaxon sfCategory)
        {
            var sfChildCategories = sfCategories.Where(x => x.ParentTaxonId == sfCategory.Id);

            foreach (var sfChildCategory in sfChildCategories)
            {
                var destCategory = category.FirstOrDefault(
                    x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == SitefinityUniqueIdPropertyName && prop.Value == sfCategory.Id.ToString()) == 1);
                var destChildCategory = category.FirstOrDefault(
                    x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == SitefinityUniqueIdPropertyName && prop.Value == sfChildCategory.Id.ToString()) == 1);

                if (destCategory != null && destChildCategory != null)
                {
                    destCategory.AddCategory(destChildCategory);
                }
            }
        }
    }
}
