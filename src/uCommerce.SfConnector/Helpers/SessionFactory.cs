using System.Data;
using System.Data.SqlClient;

namespace uCommerce.SfConnector.Helpers
{
    public static class SqlSessionFactory
    {
        public static IDbConnection Create(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
