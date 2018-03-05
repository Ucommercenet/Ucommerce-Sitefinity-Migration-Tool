using System.Collections.Generic;
using uCommerce.SfConnector.Model;
using UCommerce.EntitiesV2;
using UConnector.Framework;

namespace uCommerce.SfConnector.Transformers
{
    public class SfTaxonomyToUcTaxonomy : ITransformer<IEnumerable<SitefinityTaxonomy>, IEnumerable<Category>>
    {
        public IEnumerable<Category> Execute(IEnumerable<SitefinityTaxonomy> @from)
        {
            var tempCategories = new List<Category>();

            foreach (var sfCategory in @from)
            {
                var category = new Category();
                category.Name = sfCategory.Title_;
                
                // Description
                category.CategoryDescriptions.Add(new CategoryDescription()
                {
                    CultureCode = "en-US",  // Cultures are TODO
                    Description = sfCategory.Description,
                    DisplayName = sfCategory.Title_,
                    RenderAsContent = true,
                    Category = category
                });

                tempCategories.Add(category);

                // TODO: Part B of this exercise
                // just need to match on something within the current set ... Category Name?
                //category.ParentCategory = tempCategories.SingleOrDefault(x => x.)
                //var parentProduct = tempProducts.SingleOrDefault(x => x.Sku == product.Sku && string.IsNullOrWhiteSpace(x.VariantSku));

            }

            return tempCategories;
        }
    }
}
