using NHibernate;
using UCommerce.EntitiesV2;
using UCommerce.Infrastructure;

namespace uCommerce.uConnector.Helpers
{
    /// <summary>
    /// Session Factory
    /// </summary>
    public static class SessionFactory
    {
        /// <summary>
        /// Creates an InMemory uCommerce session  
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>an InMemory uCommerce session</returns>
        public static ISession Create(string connectionString)
        {
            return new SessionProvider(
                new InMemoryCommerceConfigurationProvider(connectionString),
                new SingleUserService("UConnector"),
                ObjectFactory.Instance.ResolveAll<IContainsNHibernateMappingsTag>()).GetSession();
        }
    }
}
