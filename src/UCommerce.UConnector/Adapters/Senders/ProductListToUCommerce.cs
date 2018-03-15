using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using UCommerce.EntitiesV2;
using uCommerce.uConnector.Helpers;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
	public class ProductListToUCommerce : Configurable, ISender<IEnumerable<Product>>
	{
		private ISession _session;

	    public string ConnectionString { private get; set; }
	    public log4net.ILog Log { private get; set; }

        public void Send(IEnumerable<Product> sourceProducts)
		{
			_session = SessionFactory.Create(ConnectionString);

			using (var tx = _session.BeginTransaction())
			{
				foreach (var sourceProduct in sourceProducts)
				{
					var destProduct = _session.Query<Product>().SingleOrDefault(a => a.Sku == sourceProduct.Sku && a.VariantSku == null);
					if (destProduct == null) // Create product
					{
						destProduct = new Product
						{
							Sku = sourceProduct.Sku,
							Name = sourceProduct.Name,
							VariantSku = null,
							ProductDefinition =
								_session.Query<ProductDefinition>().FirstOrDefault(x => x.Name == sourceProduct.ProductDefinition.Name)
						};
					    Log.Info($"adding {AbridgedName(sourceProduct.Name)} product");
						_session.SaveOrUpdate(destProduct);
					}

                    UpdateProduct(sourceProduct, destProduct); // Update relations, categories, etc.
				}
				tx.Commit();
			}
			_session.Flush();
		}

		private void UpdateProduct(Product sourceProduct, Product destProduct)
		{
			// Product
			UpdateProductValueTypes(sourceProduct, destProduct);

			// Product.Definiton ( Multilingual and Definitions )
			UpdateProductDescriptions(sourceProduct, destProduct);

			// ProductProperties
			UpdateProductProperties(sourceProduct, destProduct);

			// Prices
			UpdateProductPrices(sourceProduct, destProduct);

			// Categories
			UpdateProductCategories(sourceProduct, destProduct);

			// Variants
			UpdateProductVariants(sourceProduct, destProduct);
		}

		private void UpdateProductProperties(Product sourceProduct, Product destProduct)
		{
			var productDefinition = _session.Query<ProductDefinition>().SingleOrDefault(x => x.Name == sourceProduct.ProductDefinition.Name);
		    if (productDefinition == null)
		    {
		        return;
		    }

		    if (destProduct.ProductDefinition.Name != productDefinition.Name)
			{
				destProduct.ProductDefinition = productDefinition;
			}

			var destProductProperties = sourceProduct.ProductProperties;

			foreach (var newProperty in destProductProperties)
			{
				var currentProductProperty = destProduct.GetProperties().Cast<ProductProperty>().SingleOrDefault(
					x => !x.ProductDefinitionField.Deleted && (x.ProductDefinitionField.Name == newProperty.ProductDefinitionField.Name));

				if (currentProductProperty != null) // Update
				{
					currentProductProperty.Value = newProperty.Value;
				}
				else // Insert
				{
					var productDefinitionField =
						destProduct.GetDefinition()
							.GetDefinitionFields().Cast<ProductDefinitionField>()
							.SingleOrDefault(x => x.Name == newProperty.ProductDefinitionField.Name);

					if (productDefinitionField != null) // Field exist, insert it.
					{
						currentProductProperty = new ProductProperty
						{
							ProductDefinitionField = productDefinitionField,
							Value = newProperty.Value
						};
						destProduct.AddProductProperty(currentProductProperty);
					}
				}
			}
		}

		private void UpdateProductVariants(Product sourceProduct, Product destProduct)
		{
			var newVariants = sourceProduct.Variants;
			foreach (var newVariant in newVariants)
			{
				var currentVariant = destProduct.Variants.SingleOrDefault(x => x.VariantSku == newVariant.VariantSku);
				if (currentVariant != null) // Update
				{
					UpdateProduct(currentVariant, newVariant);
				}
				else // Insert
				{
					if (string.IsNullOrWhiteSpace(newVariant.VariantSku))
						throw new Exception("VariantSku is empty");

					var product = new Product
					{
						Sku = newVariant.Sku,
						Name = newVariant.Name,
						VariantSku = newVariant.VariantSku,
						ProductDefinition = sourceProduct.ProductDefinition
					};
					_session.Save(product);

					UpdateProduct(product, newVariant);
					sourceProduct.AddVariant(product);
				}
			}
		}

		private void UpdateProductCategories(Product sourceProduct, Product destProduct)
		{
			var newCategories = sourceProduct.CategoryProductRelations;

			foreach (var relation in newCategories)
			{
				var category = GetCategory(relation.Category);
				if (category == null)
				{
					throw new Exception(string.Format("Could not find category: {0}", relation.Category.Name));
				}

				if (!category.Products.Any(x => x.Sku == sourceProduct.Sku && x.VariantSku == sourceProduct.VariantSku))
				{
					var categoryRelation = new CategoryProductRelation();
					categoryRelation.Product = destProduct;
					categoryRelation.SortOrder = 0;
					categoryRelation.Category = category;

					category.CategoryProductRelations.Add(categoryRelation);

				    Log.Info($"  associating {AbridgedName(sourceProduct.Name)} product to category {category.Name}");
                    _session.Save(categoryRelation);
				}
			}
		}

		private Category GetCategory(Category newCategory)
		{
		    return _session.Query<Category>().SingleOrDefault(
		        x => x.CategoryProperties.Count(prop => prop.DefinitionField.Name == "SitefinityId" && prop.Value == newCategory.Name) == 1);
		}

		private void UpdateProductPrices(Product sourceProduct, Product destProduct)
		{
            var newPrices = sourceProduct.PriceGroupPrices;

			foreach (var price in newPrices)
			{
				var priceGroupPrice = destProduct.PriceGroupPrices.SingleOrDefault(a => a.PriceGroup.Name == price.PriceGroup.Name);
				if (priceGroupPrice != null) // Update
				{
					priceGroupPrice.Price = price.Price;
				}
				else // Insert
				{
					var priceGroup = _session.Query<PriceGroup>().SingleOrDefault(x => x.Name == price.PriceGroup.Name);
					if (priceGroup != null) // It exist, then insert it
					{
						price.PriceGroup = priceGroup;
						destProduct.AddPriceGroupPrice(price);
					}
				}
			}
		}

		private void UpdateProductValueTypes(Product sourceProduct, Product destProduct)
		{
			destProduct.Name = sourceProduct.Name;
			destProduct.DisplayOnSite = sourceProduct.DisplayOnSite;
			destProduct.ThumbnailImageMediaId = sourceProduct.ThumbnailImageMediaId;
			destProduct.PrimaryImageMediaId = sourceProduct.PrimaryImageMediaId;
			destProduct.Weight = sourceProduct.Weight;
			destProduct.AllowOrdering = sourceProduct.AllowOrdering;
			destProduct.Rating = sourceProduct.Rating;
		}

		private void UpdateProductDescriptions(Product sourceProduct, Product destProduct)
		{
			foreach (var productDescription in sourceProduct.ProductDescriptions)
			{
				var destProductDescription = destProduct.ProductDescriptions.SingleOrDefault(a => a.CultureCode == productDescription.CultureCode);
				if (destProductDescription != null) // Update
				{
					destProductDescription.DisplayName = productDescription.DisplayName;
					destProductDescription.ShortDescription = productDescription.ShortDescription;
					destProductDescription.LongDescription = productDescription.LongDescription;
				}
				else // Insert
				{
					destProduct.AddProductDescription(productDescription);
				}
			}
		}

	    private string AbridgedName(string name)
	    {
 	        if (name.Length > 25)
	        {
	            return name.Substring(0, 25) + "...";
	        }

	        return name;
	    }
	}
}