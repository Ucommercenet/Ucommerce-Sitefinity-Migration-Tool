using System;
using log4net;
using MigrationCommon.Data;

namespace MigrationCommandLineRunner
{
    public class ConnectivityTests
    {
        private readonly ILog _logger;

        public ConnectivityTests(ILog logger)
        {
            _logger = logger;
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
                DataHelper.TestSitefinityDatabaseConnection();
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
                DataHelper.TestUcommerceDatabaseConnection();
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
                DataHelper.TestSitefinityApiAccess();
                _logger.Info("sitefinity web services connectivity passed");
            }
            catch (Exception ex)
            {
                _logger.Error("connectivity to sitefinity web services failed.  Verify that sitefinitybaseurl, sitefinityusername and " +
                              $" sitefinitypassword are valid in app.config: {ex.Message}", ex);
                throw;
            }
        }
    }
}
