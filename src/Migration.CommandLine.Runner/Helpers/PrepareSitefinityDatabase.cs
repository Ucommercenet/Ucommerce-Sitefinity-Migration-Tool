using System.Configuration;
using System.Data;
using System.IO;
using uCommerce.SfConnector.Helpers;
using Dapper;

namespace MigrationCommandLineRunner.Helpers
{
    public static class PrepareSitefinityDatabase
    {
        public static void BuildProductToProductTypesTable()
        {
            using (var connection = SqlSessionFactory.Create(ConfigurationManager
                .ConnectionStrings["SitefinityConnectionString"].ConnectionString))
            {
                var file = new FileInfo(@"PreRequisiteDbScripts\MSSQL\CreateTypesToProductsMigrationData.sql");
                var sql = file.OpenText().ReadToEnd();

                connection.Execute(sql, commandType: CommandType.Text);
            }
        }
    }
}
