using System;
using System.Configuration;
using System.Reflection;

namespace uCommerce.uConnector.Helpers
{
    public class SettingsHelper
    {
        private static object _Lock = new object();

        private static SettingsHelper _Instance;
        public static SettingsHelper Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock(_Lock)
                    {
                        if(_Instance == null)
                            _Instance = new SettingsHelper();
                    }
                }
                return _Instance;
            }
        }
        public string ConnectionStringName { get; set; }
        public SettingsHelper()
        {
            ConnectionStringName = "UCommerce";
        }

        public void Init()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[Environment.MachineName];
            if (connectionString != null)
            {
                SetConnectionString(connectionString);
            }
        }

        private void SetConnectionString(ConnectionStringSettings connectionString)
        {
            var settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(settings, false);
            settings.ConnectionString = connectionString.ConnectionString;
        }
    }
}