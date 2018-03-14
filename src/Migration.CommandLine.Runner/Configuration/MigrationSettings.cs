﻿using System.Configuration;

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
        /// The default uCommerce catalog group name to be created that will also house catalogs
        /// </summary>
        [ConfigurationProperty("DefaultUcommerceCatalogGroupName", IsRequired = true)]
        public string DefaultUcommerceCatalogGroupName => this["DefaultUcommerceCatalogGroupName"] as string;

        /// <summary>
        /// The default uCommerce catalog name to be created that will also house the taxonomy
        /// </summary>
        [ConfigurationProperty("DefaultUcommerceCatalogName", IsRequired = true)]
        public string DefaultUcommerceCatalogName => this["DefaultUcommerceCatalogName"] as string;

        /// <summary>
        /// The default uCommerce definition name to use for migrated categories
        /// </summary>
        [ConfigurationProperty("DefaultUcommerceCategoryDefinitionName", IsRequired = true)]
        public string DefaultUcommerceCategoryDefinitionName => this["DefaultUcommerceCategoryDefinitionName"] as string;

        /// <summary>
        /// The default taxonomy id that departments are associated to in Sitefinity
        /// </summary>
        [ConfigurationProperty("SitefinityDepartmentTaxonomyId", IsRequired = true)]
        public string SitefinityDepartmentTaxonomyId => this["SitefinityDepartmentTaxonomyId"] as string;

        /// <summary>
        /// The default taxonomy id that departments are associated to in Sitefinity
        /// </summary>
        [ConfigurationProperty("DefaultUcommercePriceGroupName", IsRequired = true)]
        public string DefaultUcommercePriceGroupName => this["DefaultUcommercePriceGroupName"] as string;

        /// <summary>
        /// The default taxonomy id that departments are associated to in Sitefinity
        /// </summary>
        [ConfigurationProperty("DefaultUcommerceCurrencyISOCode", IsRequired = true)]
        public string DefaultUcommerceCurrencyISOCode => this["DefaultUcommerceCurrencyISOCode"] as string;
    }
}
