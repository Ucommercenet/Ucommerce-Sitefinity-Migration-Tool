using uCommerce.uConnector.Adapters.Receivers;
using uCommerce.uConnector.Transformers;
using UConnector.Api.V1;
using UConnector.IO;
using UConnector.IO.Csv;

namespace UConnector.Samples.Operations.UCommerce.ExportLocalFile
{
	public class ExportProductsToLocalCsvFile : Operation
	{
		protected override IOperation BuildOperation()
		{
			return FluentOperationBuilder
				.Receive<ProductListFromUCommerce>()
					.WithOption(x => x.ConnectionString = "server=(local);database=SitecoreUCommerceSitecore_Web;integrated security=SSPI;")
				.Transform<ProductListToDataTable>()
				.Transform<FromDataTableToCsvStream>()
				.Transform<StreamToWorkfileWithTimestampName>()
					.WithOption(a => a.Extension = ".csv")
				.Batch(size: 1)
				.Send<FilesToLocalDirectory>()
					.WithOption(x => x.Overwrite = true)
				.ToOperation();
		}
	}
}
