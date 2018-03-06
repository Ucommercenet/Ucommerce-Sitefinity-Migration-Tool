using System.Configuration;

namespace MigrationCommandLineRunner.Configuration
{
    public class MigrationSettings
    {
        public static MigrationSettingsSection Settings = ConfigurationManager.GetSection("migrationSettings") as MigrationSettingsSection;

        public static MigrationSettingsSection GetMigrationSettings()
        {
            return Settings;
        }
    }

    public class MigrationSettingsSection : ConfigurationSection
    {
        /// <summary>
        /// The default uCommerce catalog name to be created that will also house the taxonomy
        /// Note: this will most likely get refactored out for multi-catalog
        /// </summary>
        [ConfigurationProperty("DefaultCatalogName", IsRequired = true)]
        public string DefaultCatalogName => this["DefaultCatalogName"] as string;

        /// <summary>
        /// The default uCommerce definition name to use for migrated categories
        /// </summary>
        [ConfigurationProperty("DefaultCategoryDefinitionName", IsRequired = true)]
        public string DefaultCategoryDefinitionName => this["DefaultCategoryDefinitionName"] as string;
    }
}
