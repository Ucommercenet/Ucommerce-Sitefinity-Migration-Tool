using uCommerce.uConnector.Adapters.Senders;
using uCommerce.uConnector.Transformers;
using UConnector.Api.V1;
using UConnector.IO;
using UConnector.IO.Csv;

namespace UConnector.Samples.Operations.UCommerce.ImportLocalFile
{
	public class ImportLocalCsvFile : Operation
	{
		protected override IOperation BuildOperation()
		{
			return FluentOperationBuilder
				.Receive<FilesFromLocalDirectory>()
					.WithOption(x => x.Pattern = "*.csv")
				.Debatch()
				.Transform<WorkFileToStream>()
				.Transform<FromCsvStreamToDataTable>()
				.Transform<DataTableToProductList>()
				.Send<ProductListToUCommerce>()
					.WithOption(x => x.ConnectionString = "server=.;database=U4;integrated security=SSPI;")
				.ToOperation();
		}
	}
}
