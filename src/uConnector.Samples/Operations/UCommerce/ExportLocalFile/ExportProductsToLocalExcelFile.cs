using uCommerce.uConnector.Adapters.Receivers;
using uCommerce.uConnector.Transformers;
using UConnector.Api.V1;
using UConnector.IO;
using UConnector.IO.Excel;

namespace UConnector.Samples.Operations.UCommerce.ExportLocalFile
{
	public class ExportProductsToLocalExcelFile : Operation
	{
		protected override IOperation BuildOperation()
		{
			return FluentOperationBuilder
				.Receive<ProductListFromUCommerce>()
					.WithOption(x => x.ConnectionString = "server=.;database=u7dev;integrated security=SSPI;")
				.Transform<ProductListToDataTable>()
				.Transform<FromDataTableToExcelStream>()
				.Transform<StreamToWorkfileWithTimestampName>()
					.WithOption(a => a.Extension = ".xlsx")
				.Batch(size: 1)
				.Send<FilesToLocalDirectory>()
					.WithOption(x => x.Overwrite = true)
				.ToOperation();
		}
	}
}
