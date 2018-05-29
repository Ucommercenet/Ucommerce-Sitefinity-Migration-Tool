using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Dapper;
using MigrationCommon.Configuration;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.Model;
using timw255.Sitefinity.RestClient.ServiceWrappers.Configuration;

namespace MigrationCommon.Data
{
    public static class DataHelper
    {
        #region Sitefinity
        public static int GetProductCount()
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            using (var connection = SqlSessionFactory.Create(sitefinityConnectionString))
            {
                return connection.ExecuteScalar<int>("select count(*) from sf_ec_product where status = 0");
            }
        }

        public static IEnumerable<Guid> GetProductCategoryAssociations(string productId)
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            using (var connection = SqlSessionFactory.Create(sitefinityConnectionString))
            {
                return connection.Query<Guid>(
                    "select taxa.id from sf_ec_product prod " +
                    "join sf_ec_product_department dept on prod.id = dept.id " +
                    "join sf_taxa taxa on dept.val = taxa.id " +
                    $"where prod.id = '{productId}'");
            }
        }

        public static void TestSitefinityDatabaseConnection()
        {
            var sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            using (var connection = SqlSessionFactory.Create(sitefinityConnectionString))
            {
                connection.ExecuteScalar<int>("select count(*) from sf_sites");
            }
        }

        public static void TestSitefinityApiAccess()
        {
            var sitefinityBaseUrl = MigrationSettings.Settings.SitefinityBaseUrl;
            var sitefinityUsername = MigrationSettings.Settings.SitefinityUsername;
            var sitefinityPassword = MigrationSettings.Settings.SitefinityPassword;

            using (var sf = new SitefinityRestClient(sitefinityUsername, sitefinityPassword, sitefinityBaseUrl))
            {
                var local = new LocalizationCulturesServiceWrapper(sf);
                local.GetCultures("", 0, int.MaxValue, "", "");
            }
        }

        public static IEnumerable<CultureViewModel> GetCulturesToMigrate(SitefinityRestClient restClient)
        {
                var configuration = new ConfigSectionItemsServiceWrapper(restClient);
                var languages = configuration.GetLocalizationBasicSettings(false);
                return languages.Cultures.ToList();
        }
        #endregion

        #region UCommerce
        public static void TestUcommerceDatabaseConnection()
        {
            var uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;
            using (var connection = SqlSessionFactory.Create(uCommerceConnectionString))
            {
                connection.ExecuteScalar<int>("select count(*) from ucommerce_productcatalog");
            }
        }
        #endregion
    }
}
