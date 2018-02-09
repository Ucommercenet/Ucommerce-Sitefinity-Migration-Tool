using System.Threading;
using Common.Logging;
using UConnector.Framework;

namespace UConnector.Samples.Framework
{
	public class SimpleRetryStrategy : IRetryStrategy
	{
		private readonly ILog _log = LogManager.GetCurrentClassLogger();

		public bool Retry(int retryIterationNumber)
		{
			switch (retryIterationNumber)
			{
				case 1:
					_log.Warn("First retry. Wait 10 seconds before proceeding.");
					Thread.Sleep(10000);
					return true;
				case 2:
					_log.Warn("Second retry. Wait 30 seconds before proceeding.");
					Thread.Sleep(30000);
					return true;
				case 3:
					_log.Warn("Third retry. Wait 60 seconds before proceeding.");
					Thread.Sleep(60000);
					return true;
				default:
					_log.Warn("Default handled reached. Do not retry.");
					return false;
			}
		}
	}
}
