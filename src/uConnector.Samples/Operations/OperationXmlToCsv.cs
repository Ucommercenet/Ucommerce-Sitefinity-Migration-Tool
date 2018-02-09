using UConnector.Api.V1;
using UConnector.IO;
using UConnector.Helpers;
using UConnector.Samples.Framework;
using UConnector.Samples.Helpers;

namespace UConnector.Samples.Operations
{
	public class OperationXmlToCsv : Operation
	{
		protected override IOperation BuildOperation()
		{
			return FluentOperationBuilder
				.Receive<FilesFromLocalDirectory>()
					.WithOption(x => x.Pattern = "*.xml")
				.Debatch()
				.Transform<WorkFileToXDocument>()
				.Debatch<XDocumentToXElements>()
				.Transform<XElementToCsvRow>()
				.Batch(size: 10)
				.Send<StringsToFileInLocalDirectory>()
				.WithRetryStrategy<SimpleRetryStrategy>()
				.WithExceptionNotifier<SimpleExceptionNotifier>()
					.WithOption(x => x.EmailAddress = "some@where.com")
				.ToOperation();
		}
	}
}
