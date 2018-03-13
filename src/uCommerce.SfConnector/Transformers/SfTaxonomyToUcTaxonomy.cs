using System.Collections.Generic;
using System.Linq;
using uCommerce.SfConnector.Model;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfTaxonomyToUcTaxonomy : ITransformer<IEnumerable<SitefinityTaxonomy>, IEnumerable<Category>>
    {
        public string DefaultCatalogName { private get; set; }
        public string DefaultCategoryDefinitionName { private get; set; }

        public IEnumerable<Category> Execute(IEnumerable<SitefinityTaxonomy> @from)
        {
            var sfCategories = @from.ToList();
            var tempCategories = new List<Category>();
            var currentCatalog = new ProductCatalog {Name = DefaultCatalogName};
            var categoryDefinition = new Definition {Name = DefaultCategoryDefinitionName};

            foreach (var sfCategory in sfCategories)
            {
                // Base category
                var category = new Category
                {
                    Name = sfCategory.Title_,
                    ProductCatalog = currentCatalog,
                    Definition = categoryDefinition,
                    DisplayOnSite = true,
                };

                // Category descriptions
                category.CategoryDescriptions.Add(new CategoryDescription()
                {
                    CultureCode = "en-US",  // Cultures are TODO
                    Description = sfCategory.Description,
                    DisplayName = sfCategory.Title_,
                    RenderAsContent = true,
                    Category = category
                });

                category.AddCategoryProperty(new CategoryProperty()
                {
                    DefinitionField = GetCategoryPropertyDefinitionField("SitefinityId"),
                    Value = sfCategory.Id.ToString()
                });

                tempCategories.Add(category);
            }

            BuildParentChildCategoryRelationships(tempCategories, sfCategories);

            return tempCategories;
        }

        private DefinitionField GetCategoryPropertyDefinitionField(string definitionFieldName)
        {
            return new DefinitionField()
            {
                BuiltIn = false,
                DefaultValue = string.Empty,
                DisplayOnSite = false,
                Name = definitionFieldName,
                Multilingual = false,
                Searchable = false,
            };
        }

        private void BuildParentChildCategoryRelationships(List<Category> destCategories, List<SitefinityTaxonomy> sourceCategories)
        {
            foreach (var sourceCategory in sourceCategories)
            {
                BuildCategoryRelationship(destCategories, sourceCategories, sourceCategory);
            }
        }

        private void BuildCategoryRelationship(List<Category> destCategories, List<SitefinityTaxonomy> sourceCategories, SitefinityTaxonomy sourceCategory)
        {
            var sourceChildCategories = sourceCategories.Where(x => x.Parent_Id == sourceCategory.Id);

            foreach (var sourceChildCategory in sourceChildCategories)
            {
                var destCategory = destCategories.First(
                    x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == sourceCategory.Id.ToString()) == 1);
                var destChildCategory = destCategories.First(
                    x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == sourceChildCategory.Id.ToString()) == 1);

                if (destCategory != null && destChildCategory != null)
                {
                    destCategory.AddCategory(destChildCategory);
                }
            }
        }
    }
}
