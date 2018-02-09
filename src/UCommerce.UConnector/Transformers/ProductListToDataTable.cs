using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using UCommerce.EntitiesV2;
using uCommerce.uConnector.Model;
using UConnector.Framework;

namespace uCommerce.uConnector.Transformers
{
	public class ProductListToDataTable : ITransformer<IEnumerable<Product>, DataTable>
	{
		private readonly CultureInfo _CultureInfo = new CultureInfo("en-US");
		/// <summary>
		/// Gets or sets the category seperator. Default is "/"
		/// </summary>
		/// <value>
		/// The category seperator.
		/// </value>
		public string CategoryPartSeperator { get; set; }

		public string DateTimeFormat { get; set; }
		public string DecimalFormat { get; set; }
		public string DoubleFormat { get; set; }

		public ProductListToDataTable()
		{
			DateTimeFormat = UCommerceProduct.DATETIME_FORMAT;
			DecimalFormat = UCommerceProduct.DECIMAL_FORMAT;
			DoubleFormat = UCommerceProduct.DOUBLE_FORMAT;
			CategoryPartSeperator = UCommerceProduct.Category.CATEGORY_PART_SEPERATOR;
		}

		public DataTable Execute(IEnumerable<Product> @from)
		{
			var dataTable = new DataTable(UCommerceProduct.PRODUCTS);

			foreach (var product in @from)
			{
				AddProductToDataTable(dataTable, product);
				foreach (var variant in product.Variants)
				{
					AddProductToDataTable(dataTable, variant);
				}
			}

			return dataTable;
		}

		private void AddProductToDataTable(DataTable dataTable, Product product)
		{
			var row = dataTable.NewRow();

			AddDataToRow(row, UCommerceProduct.Columns.SKU, product.Sku);
			AddDataToRow(row, UCommerceProduct.Columns.VARIANT_SKU, product.VariantSku);
			AddDataToRow(row, UCommerceProduct.Columns.NAME, product.Name);
			AddDataToRow(row, UCommerceProduct.Columns.DISPLAY_ON_SITE, product.DisplayOnSite);
			AddDataToRow(row, UCommerceProduct.Columns.THUMBNAIL_IMAGE_MEDIA_ID, product.ThumbnailImageMediaId);
			AddDataToRow(row, UCommerceProduct.Columns.PRIMARY_IMAGE_MEDIA_ID, product.PrimaryImageMediaId);

			AddDataToRow(row, UCommerceProduct.Columns.WEIGHT, product.Weight.ToString(_CultureInfo));
			AddDataToRow(row, UCommerceProduct.Columns.ALLOW_ORDERING, product.AllowOrdering);

			AddDataToRow(row, UCommerceProduct.Columns.RATING, product.Rating.HasValue ? product.Rating.Value.ToString(_CultureInfo) : null);

			AddDataToRow(row, UCommerceProduct.Definition.GetDefinitionName(), product.ProductDefinition.Name);

			for (int i = 1; i <= 3; i++)
			{
				AddColumnToRow(row, UCommerceProduct.Category.GetName(i));
			}
			int categoryCount = 1;
			foreach (var item in product.GetCategories().Take(3))
			{
				var productCatalogGroupName = item.ProductCatalog.ProductCatalogGroup.Name;
				var productCatalogName = item.ProductCatalog.Name;
				var categoryName = item.Name;

				var list = new List<string>()
                               {
                                   productCatalogGroupName,
                                   productCatalogName,
                                   categoryName,
                               };

				var path = string.Join(CategoryPartSeperator, list);
				AddDataToRow(row, UCommerceProduct.Category.GetName(categoryCount++), path);
			}

			foreach (var item in product.PriceGroupPrices)
			{
				AddDataToRow(row, UCommerceProduct.Price.GetColumnName(item.PriceGroup.Name), item.Price.HasValue ? item.Price.Value.ToString(_CultureInfo) : null);
			}

			foreach (var productDescription in product.ProductDescriptions)
			{
				var key = productDescription.CultureCode;

				var displayName = productDescription.DisplayName;
				var shortDescription = productDescription.ShortDescription;
				var longDescription = productDescription.LongDescription;

				AddDataToRow(row, UCommerceProduct.Description.DisplayName(key), displayName);
				AddDataToRow(row, UCommerceProduct.Description.Short(key), shortDescription);
				AddDataToRow(row, UCommerceProduct.Description.Long(key), longDescription);

				AddProductDescriptionPropertiesToRow(row, productDescription.ProductDescriptionProperties);
			}

			AddProductPropertiesToRow(row, product.ProductProperties);

			dataTable.Rows.Add(row);
		}

		/// <summary>
		/// Adds the <paramref name="productProperties"/> to the row.
		/// </summary>
		/// <param name="row">The <see cref="DataRow"/> row to add the <paramref name="productProperties"/> to.</param>
		/// <param name="productProperties">The product description properties.</param>
		private void AddProductPropertiesToRow(DataRow row, IEnumerable<ProductProperty> productProperties)
		{
			foreach (var property in productProperties)
			{
				var fieldName = property.ProductDefinitionField.Name;
				var value = property.Value;
				var headerName = UCommerceProduct.Definition.GetName(fieldName);
				AddDataToRow(row, headerName, value);
			}
		}


		private void AddProductDescriptionPropertiesToRow(DataRow row, ICollection<ProductDescriptionProperty> productDescriptionProperties)
		{
			foreach (var property in productDescriptionProperties)
			{
				var fieldName = property.ProductDefinitionField.Name;
				var cultureCode = property.ProductDescription.CultureCode;
				var value = property.Value;
				var headerName = UCommerceProduct.Definition.GetName(fieldName, cultureCode);
				AddDataToRow(row, headerName, value);
			}
		}

		private void AddDataToRow(DataRow row, string column, object data)
		{
			if (!row.Table.Columns.Contains(column))
				row.Table.Columns.Add(column);

			row[column] = data;
		}

		private void AddColumnToRow(DataRow row, string column)
		{
			if (!row.Table.Columns.Contains(column))
				row.Table.Columns.Add(column);
		}
	}
}