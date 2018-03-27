using System;
using System.Configuration;
using Dapper;
using log4net;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.ServiceWrappers.Configuration;
using uCommerce.SfConnector.Configuration;
using uCommerce.SfConnector.Helpers;

namespace MigrationCommandLineRunner
{
    public class ConnectivityTests
    {
        private readonly string _sitefinityConnectionString;
        private readonly string _uCommerceConnectionString;
        private readonly string _sitefinityServicesBaseUrl;
        private readonly string _sitefinityServicesUsername;
        private readonly string _sitefinityServicesPassword;
        private readonly ILog _logger;

        public ConnectivityTests(ILog logger)
        {
            _logger = logger;
            _uCommerceConnectionString = ConfigurationManager.ConnectionStrings["UCommerceConnectionString"].ConnectionString;
            _sitefinityConnectionString = ConfigurationManager.ConnectionStrings["SitefinityConnectionString"].ConnectionString;
            _sitefinityServicesBaseUrl = MigrationSettings.Settings.SitefinityBaseUrl;
            _sitefinityServicesUsername = MigrationSettings.Settings.SitefinityUsername;
            _sitefinityServicesPassword = MigrationSettings.Settings.SitefinityPassword;
        }

        public bool TestConnections()
        {
            try
            {
                IsSitefinityDatabaseConnectionValid();
                IsUCommerceDatabaseConnectionValid();
                IsSitefinityWebServiceEndpointAndCredentialsValid();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void IsSitefinityDatabaseConnectionValid()
        {
            _logger.Info("testing connectivity to sitefinity database");
            try
            {
                using (var connection = SqlSessionFactory.Create(_sitefinityConnectionString))
                {
                    connection.Query($"select top 1 nme from sf_sites");
                }
                _logger.Info("sitefinity database connectivity passed");
            }
            catch (Exception ex)
            {
                _logger.Error($"connectivity to sitefinity database failed.  Verify that sitefinityconnectionstring is " +
                              $"valid in app.config : {ex.Message}", ex);
                throw;
            }
        }

        private void IsUCommerceDatabaseConnectionValid()
        {
            _logger.Info("testing connectivity to ucommerce database");
            try
            {
                using (var connection = SqlSessionFactory.Create(_uCommerceConnectionString))
                {
                    connection.Query($"select top 1 name from ucommerce_productcatalog");
                }
                _logger.Info("ucommerce database connectivity passed");
            }
            catch (Exception ex)
            {
                _logger.Error($"connectivity to ucommerce database failed.  Verify that ucommerceconnectionstring is " +
                              $"valid in app.config: {ex.Message}", ex);
                throw;
            }
        }

        private void IsSitefinityWebServiceEndpointAndCredentialsValid()
        {
            _logger.Info("testing connectivity to sitefinity web services");
            try
            {
                using (var sf = new SitefinityRestClient(_sitefinityServicesUsername, _sitefinityServicesPassword, _sitefinityServicesBaseUrl))
                {
                    var local = new LocalizationCulturesServiceWrapper(sf);
                    local.GetCultures("", 0, int.MaxValue, "", "");
                }
                _logger.Info("sitefinity web services connectivity passed");
            }
            catch (Exception ex)
            {
                _logger.Error($"connectivity to sitefinity web services failed.  Verify that sitefinitybaseurl, sitefinityusername and " +
                              $" sitefinitypassword are valid in app.config: {ex.Message}", ex);
                throw;
            }
        }
    }
}
