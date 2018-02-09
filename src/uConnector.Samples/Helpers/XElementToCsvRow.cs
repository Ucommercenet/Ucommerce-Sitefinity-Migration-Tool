using System.Collections.Generic;
using System.Xml.Linq;
using Common.Logging;
using UConnector.Framework;

namespace UConnector.Samples.Helpers
{
	public class XElementToCsvRow : ITransformer<XElement, string>
	{
		private readonly ILog log = LogManager.GetCurrentClassLogger();

		public string Execute(XElement input)
		{
            var data = new List<string>();

			AddDataFromXElement(input, data);
			return string.Join(",", data);
		}

		private void AddDataFromXElement(XElement element, List<string> data)
		{
			foreach (var attribute in element.Attributes())
			{
				data.Add(string.Format("'{0}'", attribute.Value));
			}

			if (!element.HasElements)
			{
				// We have reached the bottom.
				data.Add(string.Format("'{0}'", element.Value));
			}

			foreach (var childElement in element.Elements())
			{
				AddDataFromXElement(childElement, data);
			}
		}
	}
}
