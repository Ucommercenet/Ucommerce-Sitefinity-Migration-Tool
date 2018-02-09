using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using uCommerce.SfConnector.Mappings;

namespace uCommerce.SfConnector.Helpers
{
    public static class SqlSessionFactory
    {
        public static ISessionFactory CreateSession(string connectionString)
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration
                    .MsSql2012
                    .ConnectionString(connectionString).ShowSql())
                    .Mappings(m => m.FluentMappings
                    .AddFromAssemblyOf<SitefinityProductMap>())
                .BuildSessionFactory();
        }
    }
}
