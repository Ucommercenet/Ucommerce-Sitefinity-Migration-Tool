using UCommerce.Infrastructure.Configuration;

namespace uCommerce.uConnector.Helpers
{
	/// <summary>
	/// In-memory config provider used to avoid
	/// having the full XML config as part of an
	/// app.
	/// </summary>
	internal class InMemoryCommerceConfigurationProvider : CommerceConfigurationProvider
	{
		private readonly string _conncetionString;

		public InMemoryCommerceConfigurationProvider(string conncetionString)
		{
			_conncetionString = conncetionString;
		}

		public override RuntimeConfigurationSection GetRuntimeConfiguration()
		{
			return new RuntimeConfigurationSection
			{
				EnableCache = true,
				CacheProvider = "NHibernate.Caches.SysCache.SysCacheProvider, NHibernate.Caches.SysCache",
				ConnectionString = _conncetionString
			};
		}
	}
}