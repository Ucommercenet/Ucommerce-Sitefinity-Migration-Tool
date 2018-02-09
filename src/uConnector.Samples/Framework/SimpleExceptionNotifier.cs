using System;
using UConnector.Framework;

namespace UConnector.Samples.Framework
{
	public class SimpleExceptionNotifier : Configurable, IExceptionNotifier
	{
		[Required]
		public string EmailAddress { get; set; }

		public void Notify(Exception exception)
		{
			// You can make your own exception notifier.
			// In this simple example, we simply write the exception message to standard error.
			Console.Error.WriteLine("Really should send this to " + EmailAddress + " : " + exception.Message);
		}
	}
}
