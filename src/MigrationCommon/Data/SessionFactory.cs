using System.Data;
using System.Data.SqlClient;

namespace MigrationCommon.Data
{
    public static class SqlSessionFactory
    {
        public static IDbConnection Create(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
