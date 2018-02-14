using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using UCommerce.EntitiesV2;
using uCommerce.uConnector.Helpers;
using UCommerce.Infrastructure;
using UConnector.Framework;

namespace uCommerce.uConnector.Adapters.Senders
{
	public class ProductListToUCommerce : Configurable, ISender<IEnumerable<Product>>
	{
		private ISession _session;

		public void Send(IEnumerable<Product> input)
		{
			_session = SessionFactory.Create(ConnectionString);

			using (var tx = _session.BeginTransaction())
			{
				foreach (var tempProduct in input)
				{
					var product = _session.Query<Product>().SingleOrDefault(a => a.Sku == tempProduct.Sku && a.VariantSku == null);
					if (product == null) // Create product
					{
						product = new Product
						{
							Sku = tempProduct.Sku,
							Name = tempProduct.Name,
							VariantSku = null,
							ProductDefinition =
								_session.Query<ProductDefinition>().FirstOrDefault(x => x.Name == tempProduct.ProductDefinition.Name)
						};
						_session.SaveOrUpdate(product);
					}

					UpdateProduct(product, tempProduct, _session); // Update relations, categories, etc.
				}
				tx.Commit();
			}
			_session.Flush();
		}

		private void UpdateProduct(Product currentProduct, Product newProduct, ISession session)
		{
			// Product
			UpdateProductValueTypes(currentProduct, newProduct);

			// Product.Definiton ( Multilingual and Definitions )
			UpdateProductDescriptions(currentProduct, newProduct);

			// ProductProperties
			UpdateProductProperties(currentProduct, newProduct);

			// Prices
			UpdateProductPrices(currentProduct, newProduct);

			// Categories
			UpdateProductCategories(currentProduct, newProduct, session);

			// Variants
			UpdateProductVariants(currentProduct, newProduct, session);
		}

		private void UpdateProductProperties(Product currentProduct, Product newProduct)
		{
			var productDefinition = _session.Query<ProductDefinition>().SingleOrDefault(x => x.Name == newProduct.ProductDefinition.Name);
			if (productDefinition == null)
				return;

			if (currentProduct.ProductDefinition.Name != productDefinition.Name)
			{
				currentProduct.ProductDefinition = productDefinition;
			}

			var newProductProperties = newProduct.ProductProperties;

			foreach (var newProperty in newProductProperties)
			{
				var currentProductProperty = currentProduct.GetProperties().Cast<ProductProperty>().SingleOrDefault(
					x => !x.ProductDefinitionField.Deleted && (x.ProductDefinitionField.Name == newProperty.ProductDefinitionField.Name));

				if (currentProductProperty != null) // Update
				{
					currentProductProperty.Value = newProperty.Value;
				}
				else // Insert
				{
					var productDefinitionField =
						currentProduct.GetDefinition()
							.GetDefinitionFields().Cast<ProductDefinitionField>()
							.SingleOrDefault(x => x.Name == newProperty.ProductDefinitionField.Name);

					if (productDefinitionField != null) // Field exist, insert it.
					{
						currentProductProperty = new ProductProperty
						{
							ProductDefinitionField = productDefinitionField,
							Value = newProperty.Value
						};
						currentProduct.AddProductProperty(currentProductProperty);
					}
				}
			}
		}

		private void UpdateProductVariants(Product currentProduct, Product newProduct, ISession session)
		{
			var newVariants = newProduct.Variants;
			foreach (var newVariant in newVariants)
			{
				var currentVariant = currentProduct.Variants.SingleOrDefault(x => x.VariantSku == newVariant.VariantSku);
				if (currentVariant != null) // Update
				{
					UpdateProduct(currentVariant, newVariant, session);
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
						ProductDefinition = currentProduct.ProductDefinition
					};
					session.Save(product);

					UpdateProduct(product, newVariant, session);
					currentProduct.AddVariant(product);
				}
			}
		}

		private void UpdateProductCategories(Product currentProduct, Product newProduct, ISession session)
		{
			var newCategories = newProduct.CategoryProductRelations;

			foreach (var relation in newCategories)
			{
				var category = GetCategory(relation.Category);
				if (category == null)
				{
					throw new Exception(string.Format("Could not find category: {0}", relation.Category.Name));
				}

				if (!category.Products.Any(x => x.Sku == currentProduct.Sku && x.VariantSku == currentProduct.VariantSku))
				{
					var categoryRelation = new CategoryProductRelation();
					categoryRelation.Product = currentProduct;
					categoryRelation.SortOrder = 0;
					categoryRelation.Category = category;

					category.CategoryProductRelations.Add(categoryRelation);

					_session.Save(categoryRelation);
				}
			}
		}

		private Category GetCategory(Category newCategory)
		{
			if (newCategory.ProductCatalog != null && newCategory.ProductCatalog.ProductCatalogGroup != null)
			{
				return _session.Query<Category>().SingleOrDefault(x => x.Name == newCategory.Name && x.ProductCatalog.Name == newCategory.ProductCatalog.Name && x.ProductCatalog.ProductCatalogGroup.Name == newCategory.ProductCatalog.ProductCatalogGroup.Name);
			}

			if (newCategory.ProductCatalog != null)
			{
				return _session.Query<Category>().SingleOrDefault(x => x.Name == newCategory.Name && x.ProductCatalog.Name == newCategory.ProductCatalog.Name);
			}

			return _session.Query<Category>().SingleOrDefault(x => x.Name == newCategory.Name);
		}

		private void UpdateProductPrices(Product currentProduct, Product newProduct)
		{
			var newPrices = newProduct.PriceGroupPrices;

			foreach (var price in newPrices)
			{
				var priceGroupPrice = currentProduct.PriceGroupPrices.SingleOrDefault(a => a.PriceGroup.Name == price.PriceGroup.Name);
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
						currentProduct.AddPriceGroupPrice(price);
					}
				}
			}
		}

		private void UpdateProductValueTypes(Product currentProduct, Product newProduct)
		{
			currentProduct.Name = newProduct.Name;
			currentProduct.DisplayOnSite = newProduct.DisplayOnSite;
			currentProduct.ThumbnailImageMediaId = newProduct.ThumbnailImageMediaId;
			currentProduct.PrimaryImageMediaId = newProduct.PrimaryImageMediaId;
			currentProduct.Weight = newProduct.Weight;
			currentProduct.AllowOrdering = newProduct.AllowOrdering;
			currentProduct.Rating = newProduct.Rating;
		}

		private void UpdateProductDescriptions(Product currentProduct, Product newProduct)
		{
			foreach (var productDescription in newProduct.ProductDescriptions)
			{
				var currentProductDescription = currentProduct.ProductDescriptions.SingleOrDefault(a => a.CultureCode == productDescription.CultureCode);
				if (currentProductDescription != null) // Update
				{
					currentProductDescription.DisplayName = productDescription.DisplayName;
					currentProductDescription.ShortDescription = productDescription.ShortDescription;
					currentProductDescription.LongDescription = productDescription.LongDescription;
				}
				else // Insert
				{
					currentProduct.AddProductDescription(productDescription);
				}
			}
		}

		public string ConnectionString { get; set; }
	}
}